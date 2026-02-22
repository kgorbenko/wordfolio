module Wordfolio.Api.Domain.Entries.DraftOperations

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Entries.Helpers
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Transactions

let create env (parameters: CreateDraftParameters) =
    runInTransaction env (fun appEnv ->
        task {
            match validateEntryInputs parameters.EntryText parameters.Definitions parameters.Translations with
            | Error error -> return Error error
            | Ok(validText, validDefinitions, validTranslations) ->
                let! vocabularyId =
                    Operations.getOrCreateDefaultVocabulary appEnv parameters.UserId parameters.CreatedAt

                let! duplicateResult =
                    checkForDuplicate appEnv vocabularyId (validText.Trim()) parameters.AllowDuplicate

                match duplicateResult with
                | Error error -> return Error error
                | Ok() ->
                    return!
                        createEntryWithChildren
                            appEnv
                            vocabularyId
                            (validText.Trim())
                            parameters.CreatedAt
                            validDefinitions
                            validTranslations
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
                let! entries = getEntriesHierarchyByVocabularyId appEnv vocabularyId
                return Ok entries
        })

let update env userId entryId entryText (definitions: DefinitionInput list) (translations: TranslationInput list) now =
    runInTransaction env (fun appEnv ->
        task {
            match validateEntryInputs entryText definitions translations with
            | Error error -> return Error error
            | Ok(validText, validDefinitions, validTranslations) ->
                let! maybeEntry = getEntryById appEnv entryId

                match maybeEntry with
                | None -> return Error(EntryNotFound entryId)
                | Some existingEntry ->
                    let! vocabAccessResult = checkVocabularyAccess appEnv userId existingEntry.VocabularyId

                    match vocabAccessResult with
                    | Error _ -> return Error(EntryNotFound entryId)
                    | Ok _ ->
                        return!
                            updateEntryWithChildren
                                appEnv
                                entryId
                                (validText.Trim())
                                now
                                validDefinitions
                                validTranslations
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

                        return
                            match maybeUpdatedEntry with
                            | None -> failwith $"Entry {entryId} not found after move"
                            | Some updatedEntry -> Ok updatedEntry
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
