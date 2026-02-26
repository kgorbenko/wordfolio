module Wordfolio.Api.Domain.Vocabularies.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.Vocabularies.Capabilities

[<Literal>]
let MaxNameLength = 255

type GetVocabularyByIdParameters =
    { UserId: UserId
      VocabularyId: VocabularyId }

type GetVocabulariesByCollectionIdParameters =
    { UserId: UserId
      CollectionId: CollectionId }

type CreateVocabularyParameters =
    { UserId: UserId
      CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type UpdateVocabularyParameters =
    { UserId: UserId
      VocabularyId: VocabularyId
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

type DeleteVocabularyParameters =
    { UserId: UserId
      VocabularyId: VocabularyId }

let private validateName(name: string) : Result<string, VocabularyNameValidationResult> =
    if String.IsNullOrWhiteSpace(name) then
        Error VocabularyNameValidationResult.NameRequired
    elif name.Length > MaxNameLength then
        Error(VocabularyNameValidationResult.NameTooLong MaxNameLength)
    else
        Ok name

let private mapCreateValidationError(validationError: VocabularyNameValidationResult) : CreateVocabularyError =
    match validationError with
    | VocabularyNameValidationResult.NameRequired -> CreateVocabularyError.VocabularyNameRequired
    | VocabularyNameValidationResult.NameTooLong maxLength -> CreateVocabularyError.VocabularyNameTooLong maxLength

let private mapUpdateValidationError(validationError: VocabularyNameValidationResult) : UpdateVocabularyError =
    match validationError with
    | VocabularyNameValidationResult.NameRequired -> UpdateVocabularyError.VocabularyNameRequired
    | VocabularyNameValidationResult.NameTooLong maxLength -> UpdateVocabularyError.VocabularyNameTooLong maxLength

let private checkCollectionOwnership env (userId: UserId) (collectionId: CollectionId) : Task<Result<unit, unit>> =
    task {
        let! maybeCollection = getCollectionById env collectionId

        return
            match maybeCollection with
            | None -> Error()
            | Some collection ->
                if collection.UserId = userId then
                    Ok()
                else
                    Error()
    }

let getById env (parameters: GetVocabularyByIdParameters) : Task<Result<VocabularyDetail, GetVocabularyByIdError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeVocabulary = getVocabularyById appEnv parameters.VocabularyId

            match maybeVocabulary with
            | None -> return Error(GetVocabularyByIdError.VocabularyNotFound parameters.VocabularyId)
            | Some vocabulary ->
                let! maybeCollection = getCollectionById appEnv vocabulary.CollectionId

                match maybeCollection with
                | None -> return Error(GetVocabularyByIdError.VocabularyCollectionNotFound vocabulary.CollectionId)
                | Some collection ->
                    if collection.UserId <> parameters.UserId then
                        return Error(GetVocabularyByIdError.VocabularyAccessDenied parameters.VocabularyId)
                    else
                        let detail: VocabularyDetail =
                            { Id = vocabulary.Id
                              CollectionId = vocabulary.CollectionId
                              CollectionName = collection.Name
                              Name = vocabulary.Name
                              Description = vocabulary.Description
                              CreatedAt = vocabulary.CreatedAt
                              UpdatedAt = vocabulary.UpdatedAt }

                        return Ok detail
        })

let getByCollectionId
    env
    (parameters: GetVocabulariesByCollectionIdParameters)
    : Task<Result<Vocabulary list, GetVocabulariesByCollectionIdError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! ownershipResult = checkCollectionOwnership appEnv parameters.UserId parameters.CollectionId

            match ownershipResult with
            | Error() ->
                return Error(GetVocabulariesByCollectionIdError.VocabularyCollectionNotFound parameters.CollectionId)
            | Ok _ ->
                let! vocabularies = getVocabulariesByCollectionId appEnv parameters.CollectionId
                return Ok vocabularies
        })

let create env (parameters: CreateVocabularyParameters) : Task<Result<Vocabulary, CreateVocabularyError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! ownershipResult = checkCollectionOwnership appEnv parameters.UserId parameters.CollectionId

            match ownershipResult with
            | Error() -> return Error(CreateVocabularyError.VocabularyCollectionNotFound parameters.CollectionId)
            | Ok _ ->
                match validateName parameters.Name with
                | Error validationError -> return Error(mapCreateValidationError validationError)
                | Ok validName ->
                    let trimmedName = validName.Trim()

                    let createData: CreateVocabularyData =
                        { CollectionId = parameters.CollectionId
                          Name = trimmedName
                          Description = parameters.Description
                          CreatedAt = parameters.CreatedAt }

                    let! vocabularyId = createVocabulary appEnv createData
                    let! maybeVocabulary = getVocabularyById appEnv vocabularyId

                    return
                        match maybeVocabulary with
                        | Some vocabulary -> Ok vocabulary
                        | None -> failwith $"Vocabulary {vocabularyId} not found after creation"
        })

let update env (parameters: UpdateVocabularyParameters) : Task<Result<Vocabulary, UpdateVocabularyError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeVocabulary = getVocabularyById appEnv parameters.VocabularyId

            match maybeVocabulary with
            | None -> return Error(UpdateVocabularyError.VocabularyNotFound parameters.VocabularyId)
            | Some vocabulary ->
                let! ownershipResult = checkCollectionOwnership appEnv parameters.UserId vocabulary.CollectionId

                match ownershipResult with
                | Error() -> return Error(UpdateVocabularyError.VocabularyAccessDenied parameters.VocabularyId)
                | Ok _ ->
                    match validateName parameters.Name with
                    | Error validationError -> return Error(mapUpdateValidationError validationError)
                    | Ok validName ->
                        let trimmedName = validName.Trim()

                        let updateData: UpdateVocabularyData =
                            { VocabularyId = parameters.VocabularyId
                              Name = trimmedName
                              Description = parameters.Description
                              UpdatedAt = parameters.UpdatedAt }

                        let! affectedRows = updateVocabulary appEnv updateData

                        if affectedRows > 0 then
                            let updated =
                                { vocabulary with
                                    Name = trimmedName
                                    Description = parameters.Description
                                    UpdatedAt = Some parameters.UpdatedAt }

                            return Ok updated
                        else
                            return Error(UpdateVocabularyError.VocabularyNotFound parameters.VocabularyId)
        })

let delete env (parameters: DeleteVocabularyParameters) : Task<Result<unit, DeleteVocabularyError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeVocabulary = getVocabularyById appEnv parameters.VocabularyId

            match maybeVocabulary with
            | None -> return Error(DeleteVocabularyError.VocabularyNotFound parameters.VocabularyId)
            | Some vocabulary ->
                let! ownershipResult = checkCollectionOwnership appEnv parameters.UserId vocabulary.CollectionId

                match ownershipResult with
                | Error() -> return Error(DeleteVocabularyError.VocabularyAccessDenied parameters.VocabularyId)
                | Ok _ ->
                    let! affectedRows = deleteVocabulary appEnv parameters.VocabularyId

                    if affectedRows > 0 then
                        return Ok()
                    else
                        return Error(DeleteVocabularyError.VocabularyNotFound parameters.VocabularyId)
        })
