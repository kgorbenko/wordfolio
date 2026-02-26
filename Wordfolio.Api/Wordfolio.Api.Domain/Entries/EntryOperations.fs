module Wordfolio.Api.Domain.Entries.EntryOperations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Entries.Helpers

type GetByVocabularyIdParameters =
    { UserId: UserId
      CollectionId: CollectionId
      VocabularyId: VocabularyId }

type CreateParameters =
    { UserId: UserId
      CollectionId: CollectionId
      VocabularyId: VocabularyId
      EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list
      AllowDuplicate: bool
      CreatedAt: DateTimeOffset }

type GetByIdParameters =
    { UserId: UserId
      CollectionId: CollectionId
      VocabularyId: VocabularyId
      EntryId: EntryId }

type UpdateParameters =
    { UserId: UserId
      CollectionId: CollectionId
      VocabularyId: VocabularyId
      EntryId: EntryId
      EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list
      UpdatedAt: DateTimeOffset }

type DeleteParameters =
    { UserId: UserId
      CollectionId: CollectionId
      VocabularyId: VocabularyId
      EntryId: EntryId }

type MoveParameters =
    { UserId: UserId
      CollectionId: CollectionId
      VocabularyId: VocabularyId
      EntryId: EntryId
      TargetVocabularyId: VocabularyId
      UpdatedAt: DateTimeOffset }

let private checkVocabularyAccessInCollection env userId collectionId vocabularyId =
    task {
        let! hasAccess =
            hasVocabularyAccessInCollection
                env
                { VocabularyId = vocabularyId
                  CollectionId = collectionId
                  UserId = userId }

        return if not hasAccess then Error() else Ok()
    }

let private checkEntryBelongsToVocabulary entryId (entry: Entry) vocabularyId : Result<unit, unit> =
    if entry.VocabularyId <> vocabularyId then
        Error()
    else
        Ok()

let private mapCreateValidationError(error: EntryValidationError) : CreateEntryError =
    match error with
    | EntryValidationError.EntryTextRequired -> CreateEntryError.EntryTextRequired
    | EntryValidationError.EntryTextTooLong maxLength -> CreateEntryError.EntryTextTooLong maxLength
    | EntryValidationError.NoDefinitionsOrTranslations -> CreateEntryError.NoDefinitionsOrTranslations
    | EntryValidationError.TooManyExamples maxCount -> CreateEntryError.TooManyExamples maxCount
    | EntryValidationError.ExampleTextTooLong maxLength -> CreateEntryError.ExampleTextTooLong maxLength

let private mapUpdateValidationError(error: EntryValidationError) : UpdateEntryError =
    match error with
    | EntryValidationError.EntryTextRequired -> UpdateEntryError.EntryTextRequired
    | EntryValidationError.EntryTextTooLong maxLength -> UpdateEntryError.EntryTextTooLong maxLength
    | EntryValidationError.NoDefinitionsOrTranslations -> UpdateEntryError.NoDefinitionsOrTranslations
    | EntryValidationError.TooManyExamples maxCount -> UpdateEntryError.TooManyExamples maxCount
    | EntryValidationError.ExampleTextTooLong maxLength -> UpdateEntryError.ExampleTextTooLong maxLength

let getByVocabularyId
    env
    (parameters: GetByVocabularyIdParameters)
    : Task<Result<Entry list, GetEntriesByVocabularyIdError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult =
                checkVocabularyAccessInCollection
                    appEnv
                    parameters.UserId
                    parameters.CollectionId
                    parameters.VocabularyId

            match accessResult with
            | Error() ->
                return Error(GetEntriesByVocabularyIdError.VocabularyNotFoundOrAccessDenied parameters.VocabularyId)
            | Ok _ ->
                let! entries = getEntriesHierarchyByVocabularyId appEnv parameters.VocabularyId
                return Ok entries
        })

let create env (parameters: CreateParameters) : Task<Result<Entry, CreateEntryError>> =
    runInTransaction env (fun appEnv ->
        task {
            match
                validateEntryInputs
                    { EntryText = parameters.EntryText
                      Definitions = parameters.Definitions
                      Translations = parameters.Translations }
            with
            | Error error -> return Error(mapCreateValidationError error)
            | Ok(validText, validDefinitions, validTranslations) ->
                let! accessResult =
                    checkVocabularyAccessInCollection
                        appEnv
                        parameters.UserId
                        parameters.CollectionId
                        parameters.VocabularyId

                match accessResult with
                | Error() -> return Error(CreateEntryError.VocabularyNotFoundOrAccessDenied parameters.VocabularyId)
                | Ok() ->
                    let! duplicateResult =
                        checkForDuplicate
                            appEnv
                            { VocabularyId = parameters.VocabularyId
                              EntryText = validText.Trim()
                              AllowDuplicate = parameters.AllowDuplicate }

                    match duplicateResult with
                    | Error existingEntry -> return Error(CreateEntryError.DuplicateEntry existingEntry)
                    | Ok() ->
                        let! createdEntry =
                            createEntryWithChildren
                                appEnv
                                { VocabularyId = parameters.VocabularyId
                                  EntryText = validText.Trim()
                                  CreatedAt = parameters.CreatedAt
                                  Definitions = validDefinitions
                                  Translations = validTranslations }

                        return Ok createdEntry
        })

let getById env (parameters: GetByIdParameters) : Task<Result<Entry, GetEntryByIdError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult =
                checkVocabularyAccessInCollection
                    appEnv
                    parameters.UserId
                    parameters.CollectionId
                    parameters.VocabularyId

            match accessResult with
            | Error() -> return Error(GetEntryByIdError.VocabularyNotFoundOrAccessDenied parameters.VocabularyId)
            | Ok _ ->
                let! maybeEntry = getEntryById appEnv parameters.EntryId

                match maybeEntry with
                | None -> return Error(GetEntryByIdError.EntryNotFound parameters.EntryId)
                | Some entry ->
                    match checkEntryBelongsToVocabulary parameters.EntryId entry parameters.VocabularyId with
                    | Error() -> return Error(GetEntryByIdError.EntryNotFound parameters.EntryId)
                    | Ok _ -> return Ok entry
        })

let update env (parameters: UpdateParameters) : Task<Result<Entry, UpdateEntryError>> =
    runInTransaction env (fun appEnv ->
        task {
            match
                validateEntryInputs
                    { EntryText = parameters.EntryText
                      Definitions = parameters.Definitions
                      Translations = parameters.Translations }
            with
            | Error error -> return Error(mapUpdateValidationError error)
            | Ok(validText, validDefinitions, validTranslations) ->
                let! accessResult =
                    checkVocabularyAccessInCollection
                        appEnv
                        parameters.UserId
                        parameters.CollectionId
                        parameters.VocabularyId

                match accessResult with
                | Error() -> return Error(UpdateEntryError.VocabularyNotFoundOrAccessDenied parameters.VocabularyId)
                | Ok _ ->
                    let! maybeEntry = getEntryById appEnv parameters.EntryId

                    match maybeEntry with
                    | None -> return Error(UpdateEntryError.EntryNotFound parameters.EntryId)
                    | Some existingEntry ->
                        match
                            checkEntryBelongsToVocabulary parameters.EntryId existingEntry parameters.VocabularyId
                        with
                        | Error() -> return Error(UpdateEntryError.EntryNotFound parameters.EntryId)
                        | Ok _ ->
                            let! updatedEntry =
                                updateEntryWithChildren
                                    appEnv
                                    { EntryId = parameters.EntryId
                                      EntryText = validText.Trim()
                                      UpdatedAt = parameters.UpdatedAt
                                      Definitions = validDefinitions
                                      Translations = validTranslations }

                            return Ok updatedEntry
        })

let delete env (parameters: DeleteParameters) : Task<Result<unit, DeleteEntryError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult =
                checkVocabularyAccessInCollection
                    appEnv
                    parameters.UserId
                    parameters.CollectionId
                    parameters.VocabularyId

            match accessResult with
            | Error() -> return Error(DeleteEntryError.VocabularyNotFoundOrAccessDenied parameters.VocabularyId)
            | Ok _ ->
                let! maybeEntry = getEntryById appEnv parameters.EntryId

                match maybeEntry with
                | None -> return Error(DeleteEntryError.EntryNotFound parameters.EntryId)
                | Some entry ->
                    match checkEntryBelongsToVocabulary parameters.EntryId entry parameters.VocabularyId with
                    | Error() -> return Error(DeleteEntryError.EntryNotFound parameters.EntryId)
                    | Ok _ ->
                        let! _ = deleteEntry appEnv parameters.EntryId
                        return Ok()
        })

let move env (parameters: MoveParameters) : Task<Result<Entry, MoveEntryError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! accessResult =
                checkVocabularyAccessInCollection
                    appEnv
                    parameters.UserId
                    parameters.CollectionId
                    parameters.VocabularyId

            match accessResult with
            | Error() -> return Error(MoveEntryError.VocabularyNotFoundOrAccessDenied parameters.VocabularyId)
            | Ok _ ->
                let! maybeEntry = getEntryById appEnv parameters.EntryId

                match maybeEntry with
                | None -> return Error(MoveEntryError.EntryNotFound parameters.EntryId)
                | Some entry ->
                    match checkEntryBelongsToVocabulary parameters.EntryId entry parameters.VocabularyId with
                    | Error() -> return Error(MoveEntryError.EntryNotFound parameters.EntryId)
                    | Ok _ ->
                        let! targetAccessResult =
                            checkVocabularyAccess
                                appEnv
                                { UserId = parameters.UserId
                                  VocabularyId = parameters.TargetVocabularyId }

                        match targetAccessResult with
                        | Error() ->
                            return Error(MoveEntryError.VocabularyNotFoundOrAccessDenied parameters.TargetVocabularyId)
                        | Ok _ ->
                            do!
                                moveEntry
                                    appEnv
                                    { EntryId = parameters.EntryId
                                      OldVocabularyId = entry.VocabularyId
                                      NewVocabularyId = parameters.TargetVocabularyId
                                      UpdatedAt = parameters.UpdatedAt }

                            let! maybeUpdatedEntry = getEntryById appEnv parameters.EntryId

                            return
                                match maybeUpdatedEntry with
                                | None -> failwith $"Entry {parameters.EntryId} not found after move"
                                | Some updatedEntry -> Ok updatedEntry
        })
