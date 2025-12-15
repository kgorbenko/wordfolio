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
        Ok(name.Trim())

let private checkOwnership (userId: UserId) (collection: Collection) : Result<Collection, CollectionError> =
    if collection.UserId = userId then
        Ok collection
    else
        Error(CollectionAccessDenied collection.Id)

let getById env userId collectionId =
    task {
        let! maybeCollection = getCollectionById env collectionId

        return
            match maybeCollection with
            | None -> Error(CollectionNotFound collectionId)
            | Some collection -> checkOwnership userId collection
    }

let getByUserId env userId = getCollectionsByUserId env userId

let create env userId name description now =
    task {
        match validateName name with
        | Error error -> return Error error
        | Ok validName ->
            let! collection = createCollection env userId validName description now
            return Ok collection
    }

let update env userId collectionId name description now =
    task {
        let! maybeCollection = getCollectionById env collectionId

        match maybeCollection with
        | None -> return Error(CollectionNotFound collectionId)
        | Some collection ->
            match checkOwnership userId collection with
            | Error error -> return Error error
            | Ok _ ->
                match validateName name with
                | Error error -> return Error error
                | Ok validName ->
                    let! _ = updateCollection env collectionId validName description now

                    let updated =
                        { collection with
                            Name = validName
                            Description = description
                            UpdatedAt = Some now }

                    return Ok updated
    }

let delete env userId collectionId =
    task {
        let! maybeCollection = getCollectionById env collectionId

        match maybeCollection with
        | None -> return Error(CollectionNotFound collectionId)
        | Some collection ->
            match checkOwnership userId collection with
            | Error error -> return Error error
            | Ok _ ->
                let! _ = deleteCollection env collectionId
                return Ok()
    }
