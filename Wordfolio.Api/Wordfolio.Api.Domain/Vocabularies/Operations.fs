module Wordfolio.Api.Domain.Vocabularies.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain.Vocabularies.Capabilities
open Wordfolio.Api.Domain.Transactions

[<Literal>]
let MaxNameLength = 255

[<Literal>]
let SystemCollectionName =
    "[System] Unsorted"

[<Literal>]
let DefaultVocabularyName = "[Default]"

let private validateName(name: string) : Result<string, VocabularyError> =
    if String.IsNullOrWhiteSpace(name) then
        Error VocabularyNameRequired
    elif name.Length > MaxNameLength then
        Error(VocabularyNameTooLong MaxNameLength)
    else
        Ok name

let private checkCollectionOwnership env userId collectionId =
    task {
        let! maybeCollection = getCollectionById env collectionId

        return
            match maybeCollection with
            | None -> Error(VocabularyCollectionNotFound collectionId)
            | Some collection ->
                if collection.UserId = userId then
                    Ok collection
                else
                    Error(VocabularyCollectionNotFound collectionId)
    }

let getById env userId vocabularyId =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeVocabulary = getVocabularyById appEnv vocabularyId

            match maybeVocabulary with
            | None -> return Error(VocabularyNotFound vocabularyId)
            | Some vocabulary ->
                let! ownershipResult = checkCollectionOwnership appEnv userId vocabulary.CollectionId

                return
                    match ownershipResult with
                    | Error _ -> Error(VocabularyAccessDenied vocabularyId)
                    | Ok _ -> Ok vocabulary
        })

let getByCollectionId env userId collectionId =
    runInTransaction env (fun appEnv ->
        task {
            let! ownershipResult = checkCollectionOwnership appEnv userId collectionId

            match ownershipResult with
            | Error error -> return Error error
            | Ok _ ->
                let! vocabularies = getVocabulariesByCollectionId appEnv collectionId
                return Ok vocabularies
        })

let create env userId collectionId name description now =
    runInTransaction env (fun appEnv ->
        task {
            let! ownershipResult = checkCollectionOwnership appEnv userId collectionId

            match ownershipResult with
            | Error error -> return Error error
            | Ok _ ->
                match validateName name with
                | Error error -> return Error error
                | Ok validName ->
                    let trimmedName = validName.Trim()
                    let! vocabularyId = createVocabulary appEnv collectionId trimmedName description now
                    let! maybeVocabulary = getVocabularyById appEnv vocabularyId

                    match maybeVocabulary with
                    | Some vocabulary -> return Ok vocabulary
                    | None -> return Error VocabularyNameRequired
        })

let update env userId vocabularyId name description now =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeVocabulary = getVocabularyById appEnv vocabularyId

            match maybeVocabulary with
            | None -> return Error(VocabularyNotFound vocabularyId)
            | Some vocabulary ->
                let! ownershipResult = checkCollectionOwnership appEnv userId vocabulary.CollectionId

                match ownershipResult with
                | Error _ -> return Error(VocabularyAccessDenied vocabularyId)
                | Ok _ ->
                    match validateName name with
                    | Error error -> return Error error
                    | Ok validName ->
                        let trimmedName = validName.Trim()
                        let! affectedRows = updateVocabulary appEnv vocabularyId trimmedName description now

                        if affectedRows > 0 then
                            let updated =
                                { vocabulary with
                                    Name = trimmedName
                                    Description = description
                                    UpdatedAt = Some now }

                            return Ok updated
                        else
                            return Error(VocabularyNotFound vocabularyId)
        })

let delete env userId vocabularyId =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeVocabulary = getVocabularyById appEnv vocabularyId

            match maybeVocabulary with
            | None -> return Error(VocabularyNotFound vocabularyId)
            | Some vocabulary ->
                let! ownershipResult = checkCollectionOwnership appEnv userId vocabulary.CollectionId

                match ownershipResult with
                | Error _ -> return Error(VocabularyAccessDenied vocabularyId)
                | Ok _ ->
                    let! affectedRows = deleteVocabulary appEnv vocabularyId

                    if affectedRows > 0 then
                        return Ok()
                    else
                        return Error(VocabularyNotFound vocabularyId)
        })

let getDefaultOrCreate env userId now =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeVocabulary = getDefaultVocabulary appEnv userId

            match maybeVocabulary with
            | Some vocabulary -> return Ok vocabulary
            | None ->
                let! maybeCollection = getDefaultCollection appEnv userId

                let! collectionId =
                    match maybeCollection with
                    | Some collection -> collection.Id |> Task.FromResult
                    | None ->
                        let collectionParams: CreateCollectionParameters =
                            { UserId = userId
                              Name = SystemCollectionName
                              Description = None
                              CreatedAt = now }

                        createDefaultCollection appEnv collectionParams

                let vocabularyParams: CreateVocabularyParameters =
                    { CollectionId = collectionId
                      Name = DefaultVocabularyName
                      Description = None
                      CreatedAt = now }

                let! vocabularyId = createDefaultVocabulary appEnv vocabularyParams

                let vocabulary: Vocabulary =
                    { Id = vocabularyId
                      CollectionId = collectionId
                      Name = DefaultVocabularyName
                      Description = None
                      CreatedAt = now
                      UpdatedAt = None }

                return Ok vocabulary
        })
