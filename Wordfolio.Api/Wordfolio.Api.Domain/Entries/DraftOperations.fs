module Wordfolio.Api.Domain.Entries.DraftOperations

open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Entries.Helpers
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Transactions

let private checkVocabularyAccess env userId vocabularyId =
    task {
        let! hasAccess = hasVocabularyAccess env vocabularyId userId

        return
            if not hasAccess then
                Error(VocabularyNotFoundOrAccessDenied vocabularyId)
            else
                Ok()
    }

let create env (parameters: CreateDraftParameters) =
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
                    match validateDefinitions parameters.Definitions with
                    | Error error -> return Error error
                    | Ok validDefinitions ->
                        match validateTranslations parameters.Translations with
                        | Error error -> return Error error
                        | Ok validTranslations ->
                            let! vocabularyId =
                                Operations.getOrCreateDefaultVocabulary appEnv parameters.UserId parameters.CreatedAt

                            let! shouldProceed =
                                if parameters.AllowDuplicate then
                                    Task.FromResult(Ok())
                                else
                                    task {
                                        let! maybeExistingEntry =
                                            getEntryByTextAndVocabularyId appEnv vocabularyId (validText.Trim())

                                        match maybeExistingEntry with
                                        | Some existing ->
                                            let! maybeFullEntry = getEntryById appEnv existing.Id

                                            match maybeFullEntry with
                                            | Some fullEntry -> return Error(DuplicateEntry fullEntry)
                                            | None -> return Ok()
                                        | None -> return Ok()
                                    }

                            match shouldProceed with
                            | Error error -> return Error error
                            | Ok() ->
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
                let! entries = getEntriesHierarchyByVocabularyId appEnv vocabularyId
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
