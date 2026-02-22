module Wordfolio.Api.Domain.Vocabularies.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain.Transactions
open Wordfolio.Api.Domain.Vocabularies.Capabilities

[<Literal>]
let MaxNameLength = 255

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
                let! maybeCollection = getCollectionById appEnv vocabulary.CollectionId

                match maybeCollection with
                | None -> return Error(VocabularyCollectionNotFound vocabulary.CollectionId)
                | Some collection ->
                    if collection.UserId <> userId then
                        return Error(VocabularyAccessDenied vocabularyId)
                    else
                        return
                            Ok
                                { Id = vocabulary.Id
                                  CollectionId = vocabulary.CollectionId
                                  CollectionName = collection.Name
                                  Name = vocabulary.Name
                                  Description = vocabulary.Description
                                  CreatedAt = vocabulary.CreatedAt
                                  UpdatedAt = vocabulary.UpdatedAt }
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

                    return
                        match maybeVocabulary with
                        | Some vocabulary -> Ok vocabulary
                        | None -> failwith $"Vocabulary {vocabularyId} not found after creation"
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
