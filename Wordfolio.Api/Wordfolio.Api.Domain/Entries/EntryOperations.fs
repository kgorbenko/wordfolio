module Wordfolio.Api.Domain.Entries.EntryOperations

open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Entries.Helpers

let private checkVocabularyAccessInCollection env userId collectionId vocabularyId =
    task {
        let! hasAccess = hasVocabularyAccessInCollection env vocabularyId collectionId userId

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
            match validateEntryInputs parameters.EntryText parameters.Definitions parameters.Translations with
            | Error error -> return Error error
            | Ok(validText, validDefinitions, validTranslations) ->
                let! accessResult =
                    checkVocabularyAccessInCollection appEnv parameters.UserId collectionId parameters.VocabularyId

                match accessResult with
                | Error error -> return Error error
                | Ok() ->
                    let! duplicateResult =
                        checkForDuplicate appEnv parameters.VocabularyId (validText.Trim()) parameters.AllowDuplicate

                    match duplicateResult with
                    | Error error -> return Error error
                    | Ok() ->
                        return!
                            createEntryWithChildren
                                appEnv
                                parameters.VocabularyId
                                (validText.Trim())
                                parameters.CreatedAt
                                validDefinitions
                                validTranslations
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
            match validateEntryInputs entryText definitions translations with
            | Error error -> return Error error
            | Ok(validText, validDefinitions, validTranslations) ->
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
                            return!
                                updateEntryWithChildren
                                    appEnv
                                    entryId
                                    (validText.Trim())
                                    now
                                    validDefinitions
                                    validTranslations
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

                            return
                                match maybeUpdatedEntry with
                                | None -> failwith $"Entry {entryId} not found after move"
                                | Some updatedEntry -> Ok updatedEntry
        })
