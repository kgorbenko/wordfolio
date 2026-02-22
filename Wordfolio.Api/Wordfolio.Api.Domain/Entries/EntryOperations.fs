module Wordfolio.Api.Domain.Entries.EntryOperations

open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Entries.Helpers
open Wordfolio.Api.Domain.Transactions

let private checkVocabularyAccessInCollection env userId collectionId vocabularyId =
    task {
        let! hasAccess = hasVocabularyAccessInCollection env vocabularyId collectionId userId

        return
            if not hasAccess then
                Error(VocabularyNotFoundOrAccessDenied vocabularyId)
            else
                Ok()
    }

let private checkVocabularyAccess env userId vocabularyId =
    task {
        let! hasAccess = hasVocabularyAccess env vocabularyId userId

        return
            if not hasAccess then
                Error(VocabularyNotFoundOrAccessDenied vocabularyId)
            else
                Ok()
    }

let private checkEntryBelongsToVocabulary entryId (entry: Entry) vocabularyId =
    if entry.VocabularyId <> vocabularyId then
        Error(EntryNotFound entryId)
    else
        Ok()

let getByVocabularyId env userId collectionId vocabularyId =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult = checkVocabularyAccessInCollection appEnv userId collectionId vocabularyId

            match accessResult with
            | Error error -> return Error error
            | Ok _ ->
                let! entries = getEntriesHierarchyByVocabularyId appEnv vocabularyId
                return Ok entries
        })

let create env collectionId (parameters: CreateEntryParameters) =
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
                    match parameters.VocabularyId with
                    | None -> return Error(VocabularyNotFoundOrAccessDenied(VocabularyId 0))
                    | Some vocabularyId ->
                        let! accessResult =
                            checkVocabularyAccessInCollection appEnv parameters.UserId collectionId vocabularyId

                        match accessResult with
                        | Error error -> return Error error
                        | Ok() ->
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

let getById env userId collectionId vocabularyId entryId =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult = checkVocabularyAccessInCollection appEnv userId collectionId vocabularyId

            match accessResult with
            | Error error -> return Error error
            | Ok _ ->
                let! maybeEntry = getEntryById appEnv entryId

                match maybeEntry with
                | None -> return Error(EntryNotFound entryId)
                | Some entry ->
                    match checkEntryBelongsToVocabulary entryId entry vocabularyId with
                    | Error error -> return Error error
                    | Ok _ -> return Ok entry
        })

let update
    env
    userId
    collectionId
    vocabularyId
    entryId
    entryText
    (definitions: DefinitionInput list)
    (translations: TranslationInput list)
    now
    =
    runInTransaction env (fun appEnv ->
        task {
            match validateEntryText entryText with
            | Error error -> return Error error
            | Ok validText ->
                let! accessResult = checkVocabularyAccessInCollection appEnv userId collectionId vocabularyId

                match accessResult with
                | Error error -> return Error error
                | Ok _ ->
                    let! maybeEntry = getEntryById appEnv entryId

                    match maybeEntry with
                    | None -> return Error(EntryNotFound entryId)
                    | Some existingEntry ->
                        match checkEntryBelongsToVocabulary entryId existingEntry vocabularyId with
                        | Error error -> return Error error
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

let delete env userId collectionId vocabularyId entryId =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult = checkVocabularyAccessInCollection appEnv userId collectionId vocabularyId

            match accessResult with
            | Error error -> return Error error
            | Ok _ ->
                let! maybeEntry = getEntryById appEnv entryId

                match maybeEntry with
                | None -> return Error(EntryNotFound entryId)
                | Some entry ->
                    match checkEntryBelongsToVocabulary entryId entry vocabularyId with
                    | Error error -> return Error error
                    | Ok _ ->
                        let! _ = deleteEntry appEnv entryId
                        return Ok()
        })

let move env userId collectionId vocabularyId entryId targetVocabularyId now =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult = checkVocabularyAccessInCollection appEnv userId collectionId vocabularyId

            match accessResult with
            | Error error -> return Error error
            | Ok _ ->
                let! maybeEntry = getEntryById appEnv entryId

                match maybeEntry with
                | None -> return Error(EntryNotFound entryId)
                | Some entry ->
                    match checkEntryBelongsToVocabulary entryId entry vocabularyId with
                    | Error error -> return Error error
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
