module Wordfolio.Api.Domain.Collections.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.Collections.Capabilities

[<Literal>]
let MaxNameLength = 255

type GetCollectionByIdParameters =
    { UserId: UserId
      CollectionId: CollectionId }

type GetCollectionsByUserIdParameters = { UserId: UserId }

type CreateCollectionParameters =
    { UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type UpdateCollectionParameters =
    { UserId: UserId
      CollectionId: CollectionId
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

type DeleteCollectionParameters =
    { UserId: UserId
      CollectionId: CollectionId }

let private validateName(name: string) : Result<string, CollectionNameValidationError> =
    if String.IsNullOrWhiteSpace(name) then
        Error CollectionNameValidationError.NameRequired
    elif name.Length > MaxNameLength then
        Error(CollectionNameValidationError.NameTooLong MaxNameLength)
    else
        Ok name

let private mapCreateValidationError(validationError: CollectionNameValidationError) : CreateCollectionError =
    match validationError with
    | CollectionNameValidationError.NameRequired -> CreateCollectionError.CollectionNameRequired
    | CollectionNameValidationError.NameTooLong maxLength -> CreateCollectionError.CollectionNameTooLong maxLength

let private mapUpdateValidationError(validationError: CollectionNameValidationError) : UpdateCollectionError =
    match validationError with
    | CollectionNameValidationError.NameRequired -> UpdateCollectionError.CollectionNameRequired
    | CollectionNameValidationError.NameTooLong maxLength -> UpdateCollectionError.CollectionNameTooLong maxLength

let private checkOwnership (userId: UserId) (collection: Collection) : Result<unit, unit> =
    if collection.UserId = userId then
        Ok()
    else
        Error()

let getById env (parameters: GetCollectionByIdParameters) : Task<Result<Collection, GetCollectionByIdError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeCollection = getCollectionById appEnv parameters.CollectionId

            return
                match maybeCollection with
                | None -> Error(GetCollectionByIdError.CollectionNotFound parameters.CollectionId)
                | Some collection ->
                    match checkOwnership parameters.UserId collection with
                    | Ok() -> Ok collection
                    | Error() -> Error(GetCollectionByIdError.CollectionAccessDenied collection.Id)
        })

let getByUserId env (parameters: GetCollectionsByUserIdParameters) : Task<Result<Collection list, unit>> =
    runInTransaction env (fun appEnv ->
        task {
            let! collections = getCollectionsByUserId appEnv parameters.UserId
            return (Ok collections)
        })

let create env (parameters: CreateCollectionParameters) : Task<Result<Collection, CreateCollectionError>> =
    runInTransaction env (fun appEnv ->
        task {
            match validateName parameters.Name with
            | Error validationError -> return Error(mapCreateValidationError validationError)
            | Ok validName ->
                let trimmedName = validName.Trim()

                let createParameters: CreateCollectionData =
                    { UserId = parameters.UserId
                      Name = trimmedName
                      Description = parameters.Description
                      CreatedAt = parameters.CreatedAt }

                let! collectionId = createCollection appEnv createParameters
                let! maybeCollection = getCollectionById appEnv collectionId

                return
                    match maybeCollection with
                    | Some collection -> Ok collection
                    | None -> failwith $"Collection {collectionId} not found after creation"
        })

let update env (parameters: UpdateCollectionParameters) : Task<Result<Collection, UpdateCollectionError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeCollection = getCollectionById appEnv parameters.CollectionId

            match maybeCollection with
            | None -> return Error(UpdateCollectionError.CollectionNotFound parameters.CollectionId)
            | Some collection ->
                match checkOwnership parameters.UserId collection with
                | Error() -> return Error(UpdateCollectionError.CollectionAccessDenied collection.Id)
                | Ok() ->
                    match validateName parameters.Name with
                    | Error validationError -> return Error(mapUpdateValidationError validationError)
                    | Ok validName ->
                        let trimmedName = validName.Trim()

                        let updateParameters: UpdateCollectionData =
                            { CollectionId = parameters.CollectionId
                              Name = trimmedName
                              Description = parameters.Description
                              UpdatedAt = parameters.UpdatedAt }

                        let! affectedRows = updateCollection appEnv updateParameters

                        if affectedRows > 0 then
                            let updated =
                                { collection with
                                    Name = trimmedName
                                    Description = parameters.Description
                                    UpdatedAt = Some parameters.UpdatedAt }

                            return Ok updated
                        else
                            return Error(UpdateCollectionError.CollectionNotFound parameters.CollectionId)
        })

let delete env (parameters: DeleteCollectionParameters) : Task<Result<unit, DeleteCollectionError>> =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeCollection = getCollectionById appEnv parameters.CollectionId

            match maybeCollection with
            | None -> return Error(DeleteCollectionError.CollectionNotFound parameters.CollectionId)
            | Some collection ->
                match checkOwnership parameters.UserId collection with
                | Error() -> return Error(DeleteCollectionError.CollectionAccessDenied collection.Id)
                | Ok() ->
                    let! affectedRows = deleteCollection appEnv parameters.CollectionId

                    if affectedRows > 0 then
                        return Ok()
                    else
                        return Error(DeleteCollectionError.CollectionNotFound parameters.CollectionId)
        })
