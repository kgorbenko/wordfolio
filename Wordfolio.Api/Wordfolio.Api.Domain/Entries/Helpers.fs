module internal Wordfolio.Api.Domain.Entries.Helpers

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries.Capabilities

type CreateDefinitionsParameters =
    { EntryId: EntryId
      Definitions: DefinitionInput list }

type CreateTranslationsParameters =
    { EntryId: EntryId
      Translations: TranslationInput list }

type CheckVocabularyAccessParameters =
    { UserId: UserId
      VocabularyId: VocabularyId }

type ValidateEntryInputsParameters =
    { EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list }

type CheckForDuplicateParameters =
    { VocabularyId: VocabularyId
      EntryText: string
      AllowDuplicate: bool }

type CreateEntryWithChildrenParameters =
    { VocabularyId: VocabularyId
      EntryText: string
      CreatedAt: DateTimeOffset
      Definitions: DefinitionInput list
      Translations: TranslationInput list }

type UpdateEntryWithChildrenParameters =
    { EntryId: EntryId
      EntryText: string
      UpdatedAt: DateTimeOffset
      Definitions: DefinitionInput list
      Translations: TranslationInput list }

type EntryValidationError =
    | EntryTextRequired
    | EntryTextTooLong of maxLength: int
    | NoDefinitionsOrTranslations
    | TooManyExamples of maxCount: int
    | ExampleTextTooLong of maxLength: int

[<Literal>]
let MaxEntryTextLength = 200

[<Literal>]
let MaxExampleTextLength = 200

[<Literal>]
let MaxExamplesPerItem = 5

let validateEntryText(text: string) : Result<string, EntryValidationError> =
    if String.IsNullOrWhiteSpace(text) then
        Error EntryValidationError.EntryTextRequired
    elif text.Length > MaxEntryTextLength then
        Error(EntryValidationError.EntryTextTooLong MaxEntryTextLength)
    else
        Ok text

let validateExamples(examples: ExampleInput list) : Result<ExampleInput list, EntryValidationError> =
    if examples.Length > MaxExamplesPerItem then
        Error(EntryValidationError.TooManyExamples MaxExamplesPerItem)
    else
        let tooLongExample =
            examples
            |> List.tryFind(fun e -> e.ExampleText.Length > MaxExampleTextLength)

        match tooLongExample with
        | Some _ -> Error(EntryValidationError.ExampleTextTooLong MaxExampleTextLength)
        | None -> Ok examples

let private validateDefinitions
    (definitions: DefinitionInput list)
    : Result<DefinitionInput list, EntryValidationError> =
    definitions
    |> List.map(fun d ->
        validateExamples d.Examples
        |> Result.map(fun _ -> d))
    |> List.fold
        (fun acc result ->
            match acc, result with
            | Ok list, Ok item -> Ok(item :: list)
            | Error e, _ -> Error e
            | _, Error e -> Error e)
        (Ok [])
    |> Result.map List.rev

let private validateTranslations
    (translations: TranslationInput list)
    : Result<TranslationInput list, EntryValidationError> =
    translations
    |> List.map(fun t ->
        validateExamples t.Examples
        |> Result.map(fun _ -> t))
    |> List.fold
        (fun acc result ->
            match acc, result with
            | Ok list, Ok item -> Ok(item :: list)
            | Error e, _ -> Error e
            | _, Error e -> Error e)
        (Ok [])
    |> Result.map List.rev

let private createDefinitionsAsync env (parameters: CreateDefinitionsParameters) : Task<unit> =
    task {
        for i in 0 .. parameters.Definitions.Length - 1 do
            let def = parameters.Definitions[i]

            let! defId =
                createDefinition
                    env
                    { EntryId = parameters.EntryId
                      Text = def.DefinitionText
                      Source = def.Source
                      DisplayOrder = i }

            if not def.Examples.IsEmpty then
                do! createExamplesForDefinition env defId def.Examples
    }

let private createTranslationsAsync env (parameters: CreateTranslationsParameters) : Task<unit> =
    task {
        for i in 0 .. parameters.Translations.Length - 1 do
            let trans = parameters.Translations[i]

            let! transId =
                createTranslation
                    env
                    { EntryId = parameters.EntryId
                      Text = trans.TranslationText
                      Source = trans.Source
                      DisplayOrder = i }

            if not trans.Examples.IsEmpty then
                do! createExamplesForTranslation env transId trans.Examples
    }

let checkVocabularyAccess env (parameters: CheckVocabularyAccessParameters) : Task<Result<unit, unit>> =
    task {
        let! hasAccess = hasVocabularyAccess env (parameters.VocabularyId, parameters.UserId)

        return if not hasAccess then Error() else Ok()
    }

let validateEntryInputs
    (parameters: ValidateEntryInputsParameters)
    : Result<string * DefinitionInput list * TranslationInput list, EntryValidationError> =
    match validateEntryText parameters.EntryText with
    | Error error -> Error error
    | Ok validText ->
        if
            parameters.Definitions.IsEmpty
            && parameters.Translations.IsEmpty
        then
            Error EntryValidationError.NoDefinitionsOrTranslations
        else
            match validateDefinitions parameters.Definitions with
            | Error error -> Error error
            | Ok validDefinitions ->
                match validateTranslations parameters.Translations with
                | Error error -> Error error
                | Ok validTranslations -> Ok(validText, validDefinitions, validTranslations)

let checkForDuplicate env (parameters: CheckForDuplicateParameters) : Task<Result<unit, Entry>> =
    if parameters.AllowDuplicate then
        Task.FromResult(Ok())
    else
        task {
            let! maybeExistingEntry = getEntryByTextAndVocabularyId env (parameters.VocabularyId, parameters.EntryText)

            match maybeExistingEntry with
            | Some existing ->
                let! maybeFullEntry = getEntryById env existing.Id

                match maybeFullEntry with
                | Some fullEntry -> return Error fullEntry
                | None -> return Ok()
            | None -> return Ok()
        }

let createEntryWithChildren env (parameters: CreateEntryWithChildrenParameters) : Task<Entry> =
    task {
        let! entryId =
            createEntry
                env
                { VocabularyId = parameters.VocabularyId
                  EntryText = parameters.EntryText
                  CreatedAt = parameters.CreatedAt }

        do!
            createTranslationsAsync
                env
                { EntryId = entryId
                  Translations = parameters.Translations }

        do!
            createDefinitionsAsync
                env
                { EntryId = entryId
                  Definitions = parameters.Definitions }

        let! maybeEntry = getEntryById env entryId

        return
            match maybeEntry with
            | Some entry -> entry
            | None -> failwith $"Entry {entryId} not found after creation"
    }

let updateEntryWithChildren env (parameters: UpdateEntryWithChildrenParameters) : Task<Entry> =
    task {
        do! clearEntryChildren env parameters.EntryId

        do!
            updateEntry
                env
                { EntryId = parameters.EntryId
                  EntryText = parameters.EntryText
                  UpdatedAt = parameters.UpdatedAt }

        do!
            createTranslationsAsync
                env
                { EntryId = parameters.EntryId
                  Translations = parameters.Translations }

        do!
            createDefinitionsAsync
                env
                { EntryId = parameters.EntryId
                  Definitions = parameters.Definitions }

        let! maybeUpdated = getEntryById env parameters.EntryId

        return
            match maybeUpdated with
            | Some entry -> entry
            | None -> failwith $"Entry {parameters.EntryId} not found after update"
    }
