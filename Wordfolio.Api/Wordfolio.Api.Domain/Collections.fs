module Wordfolio.Api.Domain.Collections

open System
open System.Threading
open System.Threading.Tasks

[<Literal>]
let MaxNameLength = 255

type Collection =
    { Id: CollectionId
      UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateCollectionCommand =
    { UserId: UserId
      Name: string
      Description: string option }

type UpdateCollectionCommand =
    { CollectionId: CollectionId
      UserId: UserId
      Name: string
      Description: string option }

type DeleteCollectionCommand =
    { CollectionId: CollectionId
      UserId: UserId }

let private fromData(data: CollectionData) : Collection =
    { Id = data.Id
      UserId = data.UserId
      Name = data.Name
      Description = data.Description
      CreatedAt = data.CreatedAt
      UpdatedAt = data.UpdatedAt }

let private validateName(name: string) : Result<string, CollectionError> =
    if String.IsNullOrWhiteSpace(name) then
        Error CollectionNameRequired
    elif name.Length > MaxNameLength then
        Error(CollectionNameTooLong MaxNameLength)
    else
        Ok(name.Trim())

let private checkOwnership (userId: UserId) (collection: CollectionData) : Result<CollectionData, CollectionError> =
    if collection.UserId = userId then
        Ok collection
    else
        Error(CollectionAccessDenied collection.Id)

let getByIdAsync
    (repository: ICollectionRepository)
    (userId: UserId)
    (collectionId: CollectionId)
    (cancellationToken: CancellationToken)
    : Task<Result<Collection, CollectionError>> =
    task {
        let! maybeCollection = repository.GetByIdAsync(collectionId, cancellationToken)

        return
            match maybeCollection with
            | None -> Error(CollectionNotFound collectionId)
            | Some collection ->
                collection
                |> checkOwnership userId
                |> Result.map fromData
    }

let getByUserIdAsync
    (repository: ICollectionRepository)
    (userId: UserId)
    (cancellationToken: CancellationToken)
    : Task<Collection list> =
    task {
        let! collections = repository.GetByUserIdAsync(userId, cancellationToken)
        return collections |> List.map fromData
    }

let createAsync
    (repository: ICollectionRepository)
    (command: CreateCollectionCommand)
    (now: DateTimeOffset)
    (cancellationToken: CancellationToken)
    : Task<Result<Collection, CollectionError>> =
    task {
        match validateName command.Name with
        | Error error -> return Error error
        | Ok validName ->
            let! data =
                repository.CreateAsync(
                    command.UserId,
                    validName,
                    command.Description,
                    now,
                    cancellationToken
                )

            return Ok(fromData data)
    }

let updateAsync
    (repository: ICollectionRepository)
    (command: UpdateCollectionCommand)
    (now: DateTimeOffset)
    (cancellationToken: CancellationToken)
    : Task<Result<Collection, CollectionError>> =
    task {
        let! maybeCollection = repository.GetByIdAsync(command.CollectionId, cancellationToken)

        match maybeCollection with
        | None -> return Error(CollectionNotFound command.CollectionId)
        | Some collection ->
            match checkOwnership command.UserId collection with
            | Error error -> return Error error
            | Ok _ ->
                match validateName command.Name with
                | Error error -> return Error error
                | Ok validName ->
                    let! _ =
                        repository.UpdateAsync(
                            command.CollectionId,
                            validName,
                            command.Description,
                            now,
                            cancellationToken
                        )

                    let updated =
                        { collection with
                            Name = validName
                            Description = command.Description
                            UpdatedAt = Some now }

                    return Ok(fromData updated)
    }

let deleteAsync
    (repository: ICollectionRepository)
    (command: DeleteCollectionCommand)
    (cancellationToken: CancellationToken)
    : Task<Result<unit, CollectionError>> =
    task {
        let! maybeCollection = repository.GetByIdAsync(command.CollectionId, cancellationToken)

        match maybeCollection with
        | None -> return Error(CollectionNotFound command.CollectionId)
        | Some collection ->
            match checkOwnership command.UserId collection with
            | Error error -> return Error error
            | Ok _ ->
                let! _ = repository.DeleteAsync(command.CollectionId, cancellationToken)
                return Ok()
    }
