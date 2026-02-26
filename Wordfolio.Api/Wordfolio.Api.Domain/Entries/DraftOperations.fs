module Wordfolio.Api.Domain.Entries.DraftOperations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Entries.Helpers

type CreateParameters =
    { UserId: UserId
      EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list
      AllowDuplicate: bool
      CreatedAt: DateTimeOffset }

type GetByIdParameters = { UserId: UserId; EntryId: EntryId }

type GetByVocabularyIdParameters =
    { UserId: UserId
      VocabularyId: VocabularyId }

type UpdateParameters =
    { UserId: UserId
      EntryId: EntryId
      EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list
      UpdatedAt: DateTimeOffset }

type MoveParameters =
    { UserId: UserId
      EntryId: EntryId
      TargetVocabularyId: VocabularyId
      UpdatedAt: DateTimeOffset }

type DeleteParameters = { UserId: UserId; EntryId: EntryId }

type GetDraftsParameters = { UserId: UserId }

let private mapCreateValidationError(error: EntryValidationError) : CreateDraftEntryError =
    match error with
    | EntryValidationError.EntryTextRequired -> CreateDraftEntryError.EntryTextRequired
    | EntryValidationError.EntryTextTooLong maxLength -> CreateDraftEntryError.EntryTextTooLong maxLength
    | EntryValidationError.NoDefinitionsOrTranslations -> CreateDraftEntryError.NoDefinitionsOrTranslations
    | EntryValidationError.TooManyExamples maxCount -> CreateDraftEntryError.TooManyExamples maxCount
    | EntryValidationError.ExampleTextTooLong maxLength -> CreateDraftEntryError.ExampleTextTooLong maxLength

let private mapUpdateValidationError(error: EntryValidationError) : UpdateDraftEntryError =
    match error with
    | EntryValidationError.EntryTextRequired -> UpdateDraftEntryError.EntryTextRequired
    | EntryValidationError.EntryTextTooLong maxLength -> UpdateDraftEntryError.EntryTextTooLong maxLength
    | EntryValidationError.NoDefinitionsOrTranslations -> UpdateDraftEntryError.NoDefinitionsOrTranslations
    | EntryValidationError.TooManyExamples maxCount -> UpdateDraftEntryError.TooManyExamples maxCount
    | EntryValidationError.ExampleTextTooLong maxLength -> UpdateDraftEntryError.ExampleTextTooLong maxLength

let create env (parameters: CreateParameters) : Task<Result<Entry, CreateDraftEntryError>> =
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
                let! vocabularyId =
                    Operations.getOrCreateDefaultVocabulary appEnv parameters.UserId parameters.CreatedAt

                let! duplicateResult =
                    checkForDuplicate
                        appEnv
                        { VocabularyId = vocabularyId
                          EntryText = validText.Trim()
                          AllowDuplicate = parameters.AllowDuplicate }

                match duplicateResult with
                | Error existingEntry -> return Error(CreateDraftEntryError.DuplicateEntry existingEntry)
                | Ok() ->
                    let! createdEntry =
                        createEntryWithChildren
                            appEnv
                            { VocabularyId = vocabularyId
                              EntryText = validText.Trim()
                              CreatedAt = parameters.CreatedAt
                              Definitions = validDefinitions
                              Translations = validTranslations }

                    return Ok createdEntry
        })

let getById env (parameters: GetByIdParameters) : Task<Result<Entry, GetDraftEntryByIdError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeEntry = getEntryById appEnv parameters.EntryId

            match maybeEntry with
            | None -> return Error(GetDraftEntryByIdError.EntryNotFound parameters.EntryId)
            | Some entry ->
                let! vocabAccessResult =
                    checkVocabularyAccess
                        appEnv
                        { UserId = parameters.UserId
                          VocabularyId = entry.VocabularyId }

                match vocabAccessResult with
                | Error _ -> return Error(GetDraftEntryByIdError.EntryNotFound parameters.EntryId)
                | Ok _ -> return Ok entry
        })

let getByVocabularyId
    env
    (parameters: GetByVocabularyIdParameters)
    : Task<Result<Entry list, GetDraftEntriesByVocabularyIdError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! vocabAccessResult =
                checkVocabularyAccess
                    appEnv
                    { UserId = parameters.UserId
                      VocabularyId = parameters.VocabularyId }

            match vocabAccessResult with
            | Error() ->
                return
                    Error(GetDraftEntriesByVocabularyIdError.VocabularyNotFoundOrAccessDenied parameters.VocabularyId)
            | Ok _ ->
                let! entries = getEntriesHierarchyByVocabularyId appEnv parameters.VocabularyId
                return Ok entries
        })

let update env (parameters: UpdateParameters) : Task<Result<Entry, UpdateDraftEntryError>> =
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
                let! maybeEntry = getEntryById appEnv parameters.EntryId

                match maybeEntry with
                | None -> return Error(UpdateDraftEntryError.EntryNotFound parameters.EntryId)
                | Some existingEntry ->
                    let! vocabAccessResult =
                        checkVocabularyAccess
                            appEnv
                            { UserId = parameters.UserId
                              VocabularyId = existingEntry.VocabularyId }

                    match vocabAccessResult with
                    | Error _ -> return Error(UpdateDraftEntryError.EntryNotFound parameters.EntryId)
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

let move env (parameters: MoveParameters) : Task<Result<Entry, MoveDraftEntryError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeEntry = getEntryById appEnv parameters.EntryId

            match maybeEntry with
            | None -> return Error(MoveDraftEntryError.EntryNotFound parameters.EntryId)
            | Some entry ->
                let! sourceAccessResult =
                    checkVocabularyAccess
                        appEnv
                        { UserId = parameters.UserId
                          VocabularyId = entry.VocabularyId }

                match sourceAccessResult with
                | Error _ -> return Error(MoveDraftEntryError.EntryNotFound parameters.EntryId)
                | Ok _ ->
                    let! targetAccessResult =
                        checkVocabularyAccess
                            appEnv
                            { UserId = parameters.UserId
                              VocabularyId = parameters.TargetVocabularyId }

                    match targetAccessResult with
                    | Error() ->
                        return
                            Error(MoveDraftEntryError.VocabularyNotFoundOrAccessDenied parameters.TargetVocabularyId)
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

let delete env (parameters: DeleteParameters) : Task<Result<unit, DeleteDraftEntryError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeEntry = getEntryById appEnv parameters.EntryId

            match maybeEntry with
            | None -> return Error(DeleteDraftEntryError.EntryNotFound parameters.EntryId)
            | Some entry ->
                let! vocabAccessResult =
                    checkVocabularyAccess
                        appEnv
                        { UserId = parameters.UserId
                          VocabularyId = entry.VocabularyId }

                match vocabAccessResult with
                | Error _ -> return Error(DeleteDraftEntryError.EntryNotFound parameters.EntryId)
                | Ok _ ->
                    let! _ = deleteEntry appEnv parameters.EntryId
                    return Ok()
        })

let getDrafts env (parameters: GetDraftsParameters) : Task<Result<DraftsVocabularyData option, unit>> =
    runInTransaction env (fun appEnv ->
        task {
            match! getDefaultVocabulary appEnv parameters.UserId with
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
