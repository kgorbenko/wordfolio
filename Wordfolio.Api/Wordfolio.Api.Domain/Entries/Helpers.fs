module Wordfolio.Api.Domain.Entries.Helpers

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries.Capabilities

[<Literal>]
let MaxEntryTextLength = 200

[<Literal>]
let MaxExampleTextLength = 200

[<Literal>]
let MaxExamplesPerItem = 5

let validateEntryText(text: string) : Result<string, EntryError> =
    if String.IsNullOrWhiteSpace(text) then
        Error EntryTextRequired
    elif text.Length > MaxEntryTextLength then
        Error(EntryTextTooLong MaxEntryTextLength)
    else
        Ok text

let validateExamples(examples: ExampleInput list) : Result<ExampleInput list, EntryError> =
    if examples.Length > MaxExamplesPerItem then
        Error(TooManyExamples MaxExamplesPerItem)
    else
        let tooLongExample =
            examples
            |> List.tryFind(fun e -> e.ExampleText.Length > MaxExampleTextLength)

        match tooLongExample with
        | Some _ -> Error(ExampleTextTooLong MaxExampleTextLength)
        | None -> Ok examples

let validateDefinitions(definitions: DefinitionInput list) : Result<DefinitionInput list, EntryError> =
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

let validateTranslations(translations: TranslationInput list) : Result<TranslationInput list, EntryError> =
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

let createDefinitionsAsync env entryId (definitions: DefinitionInput list) : Task<unit> =
    task {
        for i in 0 .. definitions.Length - 1 do
            let def = definitions[i]

            let! defId = createDefinition env entryId def.DefinitionText def.Source i

            if not def.Examples.IsEmpty then
                do! createExamplesForDefinition env defId def.Examples
    }

let createTranslationsAsync env entryId (translations: TranslationInput list) : Task<unit> =
    task {
        for i in 0 .. translations.Length - 1 do
            let trans = translations.[i]

            let! transId = createTranslation env entryId trans.TranslationText trans.Source i

            if not trans.Examples.IsEmpty then
                do! createExamplesForTranslation env transId trans.Examples
    }

let checkVocabularyAccess env userId vocabularyId =
    task {
        let! hasAccess = hasVocabularyAccess env vocabularyId userId

        return
            if not hasAccess then
                Error(VocabularyNotFoundOrAccessDenied vocabularyId)
            else
                Ok()
    }

let validateEntryInputs
    entryText
    (definitions: DefinitionInput list)
    (translations: TranslationInput list)
    : Result<string * DefinitionInput list * TranslationInput list, EntryError> =
    match validateEntryText entryText with
    | Error error -> Error error
    | Ok validText ->
        if
            definitions.IsEmpty
            && translations.IsEmpty
        then
            Error NoDefinitionsOrTranslations
        else
            match validateDefinitions definitions with
            | Error error -> Error error
            | Ok validDefinitions ->
                match validateTranslations translations with
                | Error error -> Error error
                | Ok validTranslations -> Ok(validText, validDefinitions, validTranslations)

let checkForDuplicate env vocabularyId entryText allowDuplicate : Task<Result<unit, EntryError>> =
    if allowDuplicate then
        Task.FromResult(Ok())
    else
        task {
            let! maybeExistingEntry = getEntryByTextAndVocabularyId env vocabularyId entryText

            match maybeExistingEntry with
            | Some existing ->
                let! maybeFullEntry = getEntryById env existing.Id

                match maybeFullEntry with
                | Some fullEntry -> return Error(DuplicateEntry fullEntry)
                | None -> return Ok()
            | None -> return Ok()
        }

let createEntryWithChildren
    env
    vocabularyId
    entryText
    createdAt
    (definitions: DefinitionInput list)
    (translations: TranslationInput list)
    : Task<Result<Entry, EntryError>> =
    task {
        let! entryId = createEntry env vocabularyId entryText createdAt

        do! createTranslationsAsync env entryId translations
        do! createDefinitionsAsync env entryId definitions

        let! maybeEntry = getEntryById env entryId

        return
            match maybeEntry with
            | Some entry -> Ok entry
            | None -> Error EntryTextRequired
    }

let updateEntryWithChildren
    env
    entryId
    entryText
    now
    (definitions: DefinitionInput list)
    (translations: TranslationInput list)
    : Task<Result<Entry, EntryError>> =
    task {
        do! clearEntryChildren env entryId
        do! updateEntry env entryId entryText now

        do! createTranslationsAsync env entryId translations
        do! createDefinitionsAsync env entryId definitions

        let! maybeUpdated = getEntryById env entryId

        return
            match maybeUpdated with
            | Some entry -> Ok entry
            | None -> Error(EntryNotFound entryId)
    }
