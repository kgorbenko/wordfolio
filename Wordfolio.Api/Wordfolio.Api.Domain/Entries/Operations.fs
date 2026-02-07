module Wordfolio.Api.Domain.Entries.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Transactions

[<Literal>]
let MaxEntryTextLength = 200

[<Literal>]
let MaxExampleTextLength = 200

[<Literal>]
let MaxExamplesPerItem = 5

let private validateEntryText(text: string) : Result<string, EntryError> =
    if String.IsNullOrWhiteSpace(text) then
        Error EntryTextRequired
    elif text.Length > MaxEntryTextLength then
        Error(EntryTextTooLong MaxEntryTextLength)
    else
        Ok text

let private validateExamples(examples: ExampleInput list) : Result<ExampleInput list, EntryError> =
    if examples.Length > MaxExamplesPerItem then
        Error(TooManyExamples MaxExamplesPerItem)
    else
        let tooLongExample =
            examples
            |> List.tryFind(fun e -> e.ExampleText.Length > MaxExampleTextLength)

        match tooLongExample with
        | Some _ -> Error(ExampleTextTooLong MaxExampleTextLength)
        | None -> Ok examples

let private validateDefinitions(definitions: DefinitionInput list) : Result<DefinitionInput list, EntryError> =
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

let private validateTranslations(translations: TranslationInput list) : Result<TranslationInput list, EntryError> =
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

let private checkVocabularyAccess env userId vocabularyId =
    task {
        let! hasAccess = hasVocabularyAccess env vocabularyId userId

        return
            if not hasAccess then
                Error(VocabularyNotFoundOrAccessDenied vocabularyId)
            else
                Ok()
    }

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

let create env (parameters: CreateEntryParameters) =
    runInTransaction env (fun appEnv ->
        task {
            match validateEntryText parameters.EntryText with
            | Error error -> return Error error
            | Ok validText ->
                if
                    parameters.Definitions.IsEmpty
                    && parameters.Translations.IsEmpty
                then
                    return Error NoDefinitionsOrTranslations
                else
                    let! vocabularyIdResult =
                        match parameters.VocabularyId with
                        | Some id ->
                            task {
                                let! accessResult = checkVocabularyAccess appEnv parameters.UserId id
                                return accessResult |> Result.map(fun () -> id)
                            }
                        | None ->
                            task {
                                let! id =
                                    Operations.getOrCreateDefaultVocabulary
                                        appEnv
                                        parameters.UserId
                                        parameters.CreatedAt

                                return Ok id
                            }

                    match vocabularyIdResult with
                    | Error error -> return Error error
                    | Ok vocabularyId ->
                        let! maybeExistingEntry = getEntryByTextAndVocabularyId appEnv vocabularyId (validText.Trim())

                        match maybeExistingEntry with
                        | Some existing -> return Error(DuplicateEntry existing.Id)
                        | None ->
                            match validateDefinitions parameters.Definitions with
                            | Error error -> return Error error
                            | Ok validDefinitions ->
                                match validateTranslations parameters.Translations with
                                | Error error -> return Error error
                                | Ok validTranslations ->
                                    let trimmedText = validText.Trim()

                                    let! entryId = createEntry appEnv vocabularyId trimmedText parameters.CreatedAt

                                    do! createTranslationsAsync appEnv entryId validTranslations
                                    do! createDefinitionsAsync appEnv entryId validDefinitions

                                    let! maybeEntry = getEntryById appEnv entryId

                                    match maybeEntry with
                                    | Some entry -> return Ok entry
                                    | None -> return Error EntryTextRequired
        })

let getById env userId entryId =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeEntry = getEntryById appEnv entryId

            match maybeEntry with
            | None -> return Error(EntryNotFound entryId)
            | Some entry ->
                let! vocabAccessResult = checkVocabularyAccess appEnv userId entry.VocabularyId

                match vocabAccessResult with
                | Error _ -> return Error(EntryNotFound entryId)
                | Ok _ -> return Ok entry
        })

let getByVocabularyId env userId vocabularyId =
    runInTransaction env (fun appEnv ->
        task {
            let! vocabAccessResult = checkVocabularyAccess appEnv userId vocabularyId

            match vocabAccessResult with
            | Error error -> return Error error
            | Ok _ ->
                let! entries = getEntriesByVocabularyId appEnv vocabularyId
                return Ok entries
        })

let update env userId entryId entryText (definitions: DefinitionInput list) (translations: TranslationInput list) now =
    runInTransaction env (fun appEnv ->
        task {
            match validateEntryText entryText with
            | Error error -> return Error error
            | Ok validText ->
                let! maybeEntry = getEntryById appEnv entryId

                match maybeEntry with
                | None -> return Error(EntryNotFound entryId)
                | Some existingEntry ->
                    let! vocabAccessResult = checkVocabularyAccess appEnv userId existingEntry.VocabularyId

                    match vocabAccessResult with
                    | Error _ -> return Error(EntryNotFound entryId)
                    | Ok _ ->
                        if
                            definitions.IsEmpty
                            && translations.IsEmpty
                        then
                            return Error NoDefinitionsOrTranslations
                        else
                            let trimmedText = validText.Trim()

                            let! maybeDuplicate =
                                getEntryByTextAndVocabularyId appEnv existingEntry.VocabularyId trimmedText

                            match maybeDuplicate with
                            | Some dup when dup.Id <> entryId -> return Error(DuplicateEntry dup.Id)
                            | _ ->
                                match validateDefinitions definitions with
                                | Error error -> return Error error
                                | Ok validDefinitions ->
                                    match validateTranslations translations with
                                    | Error error -> return Error error
                                    | Ok validTranslations ->
                                        do! clearEntryChildren appEnv entryId
                                        do! updateEntry appEnv entryId trimmedText now

                                        do! createTranslationsAsync appEnv entryId validTranslations
                                        do! createDefinitionsAsync appEnv entryId validDefinitions

                                        let! maybeUpdated = getEntryById appEnv entryId

                                        match maybeUpdated with
                                        | Some entry -> return Ok entry
                                        | None -> return Error(EntryNotFound entryId)
        })

let move env userId entryId targetVocabularyId now =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeEntry = getEntryById appEnv entryId

            match maybeEntry with
            | None -> return Error(EntryNotFound entryId)
            | Some entry ->
                let! sourceAccessResult = checkVocabularyAccess appEnv userId entry.VocabularyId

                match sourceAccessResult with
                | Error _ -> return Error(EntryNotFound entryId)
                | Ok _ ->
                    let! targetAccessResult = checkVocabularyAccess appEnv userId targetVocabularyId

                    match targetAccessResult with
                    | Error _ -> return Error(VocabularyNotFoundOrAccessDenied targetVocabularyId)
                    | Ok _ ->
                        do! moveEntry appEnv entryId entry.VocabularyId targetVocabularyId now

                        let! maybeUpdatedEntry = getEntryById appEnv entryId

                        match maybeUpdatedEntry with
                        | None -> return Error(EntryNotFound entryId)
                        | Some updatedEntry -> return Ok updatedEntry
        })

let delete env userId entryId =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeEntry = getEntryById appEnv entryId

            match maybeEntry with
            | None -> return Error(EntryNotFound entryId)
            | Some entry ->
                let! vocabAccessResult = checkVocabularyAccess appEnv userId entry.VocabularyId

                match vocabAccessResult with
                | Error _ -> return Error(EntryNotFound entryId)
                | Ok _ ->
                    let! _ = deleteEntry appEnv entryId
                    return Ok()
        })

let getDrafts (env: #ITransactional<#IGetDefaultVocabulary & #IGetEntriesHierarchyByVocabularyId>) (userId: UserId) =
    runInTransaction env (fun appEnv ->
        task {
            match! Shared.Capabilities.getDefaultVocabulary appEnv userId with
            | None -> return Ok None
            | Some vocabulary ->
                let! entries = getEntriesHierarchyByVocabularyId appEnv vocabulary.Id

                return
                    Ok(
                        Some
                            { Vocabulary = vocabulary
                              Entries = entries }
                    )
        })
