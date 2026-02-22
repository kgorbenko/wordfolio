module Wordfolio.Api.Domain.Collections.Operations

open System

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections.Capabilities
open Wordfolio.Api.Domain.Transactions

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

let getById env userId collectionId =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeCollection = getCollectionById appEnv collectionId

            return
                match maybeCollection with
                | None -> Error(CollectionNotFound collectionId)
                | Some collection -> checkOwnership userId collection
        })

let getByUserId env userId =
    task {
        let! result =
            runInTransaction env (fun appEnv ->
                task {
                    let! collections = getCollectionsByUserId appEnv userId
                    return Ok collections
                })

        return
            match result with
            | Ok collections -> collections
            | Error _ -> []
    }

let create env userId name description now =
    runInTransaction env (fun appEnv ->
        task {
            match validateName name with
            | Error error -> return Error error
            | Ok validName ->
                let trimmedName = validName.Trim()
                let! collectionId = createCollection appEnv userId trimmedName description now
                let! maybeCollection = getCollectionById appEnv collectionId

                return
                    match maybeCollection with
                    | Some collection -> Ok collection
                    | None -> failwith $"Collection {collectionId} not found after creation"
        })

let update env userId collectionId name description now =
    runInTransaction env (fun appEnv ->
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

let delete env userId collectionId =
    runInTransaction env (fun appEnv ->
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
