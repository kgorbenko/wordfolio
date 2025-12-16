module Wordfolio.Api.Domain.Collections.Operations

open System

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections.Capabilities

[<Literal>]
let MaxNameLength = 255

let private validateName(name: string) : Result<string, CollectionError> =
    if String.IsNullOrWhiteSpace(name) then
        Error CollectionNameRequired
    elif name.Length > MaxNameLength then
        Error(CollectionNameTooLong MaxNameLength)
    else
        Ok name

let private checkOwnership (userId: UserId) (collection: Collection) : Result<Collection, CollectionError> =
    if collection.UserId = userId then
        Ok collection
    else
        Error(CollectionAccessDenied collection.Id)

let getById transactional userId collectionId =
    Transactions.runInTransaction transactional (fun appEnv ->
        task {
            let! maybeCollection = getCollectionById appEnv collectionId

            return
                match maybeCollection with
                | None -> Error(CollectionNotFound collectionId)
                | Some collection -> checkOwnership userId collection
        })

let getByUserId transactional userId =
    task {
        let! result =
            Transactions.runInTransaction transactional (fun appEnv ->
                task {
                    let! collections = getCollectionsByUserId appEnv userId
                    return Ok collections
                })

        return
            match result with
            | Ok collections -> collections
            | Error _ -> []
    }

let create transactional userId name description now =
    Transactions.runInTransaction transactional (fun appEnv ->
        task {
            match validateName name with
            | Error error -> return Error error
            | Ok validName ->
                let trimmedName = validName.Trim()
                let! collectionId = createCollection appEnv userId trimmedName description now
                let! maybeCollection = getCollectionById appEnv collectionId

                match maybeCollection with
                | Some collection -> return Ok collection
                | None -> return Error CollectionNameRequired
        })

let update transactional userId collectionId name description now =
    Transactions.runInTransaction transactional (fun appEnv ->
        task {
            let! maybeCollection = getCollectionById appEnv collectionId

            match maybeCollection with
            | None -> return Error(CollectionNotFound collectionId)
            | Some collection ->
                match checkOwnership userId collection with
                | Error error -> return Error error
                | Ok _ ->
                    match validateName name with
                    | Error error -> return Error error
                    | Ok validName ->
                        let trimmedName = validName.Trim()
                        let! affectedRows = updateCollection appEnv collectionId trimmedName description now

                        if affectedRows > 0 then
                            let updated =
                                { collection with
                                    Name = trimmedName
                                    Description = description
                                    UpdatedAt = Some now }

                            return Ok updated
                        else
                            return Error(CollectionNotFound collectionId)
        })

let delete transactional userId collectionId =
    Transactions.runInTransaction transactional (fun appEnv ->
        task {
            let! maybeCollection = getCollectionById appEnv collectionId

            match maybeCollection with
            | None -> return Error(CollectionNotFound collectionId)
            | Some collection ->
                match checkOwnership userId collection with
                | Error error -> return Error error
                | Ok _ ->
                    let! affectedRows = deleteCollection appEnv collectionId

                    if affectedRows > 0 then
                        return Ok()
                    else
                        return Error(CollectionNotFound collectionId)
        })
