module Wordfolio.Api.Domain.Vocabularies.Operations

open System

open Wordfolio.Api.Domain
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
    task {
        let! maybeVocabulary = getVocabularyById env vocabularyId

        match maybeVocabulary with
        | None -> return Error(VocabularyNotFound vocabularyId)
        | Some vocabulary ->
            let! ownershipResult = checkCollectionOwnership env userId vocabulary.CollectionId

            return
                match ownershipResult with
                | Error _ -> Error(VocabularyAccessDenied vocabularyId)
                | Ok _ -> Ok vocabulary
    }

let getByCollectionId env userId collectionId =
    task {
        let! ownershipResult = checkCollectionOwnership env userId collectionId

        match ownershipResult with
        | Error error -> return Error error
        | Ok _ ->
            let! vocabularies = getVocabulariesByCollectionId env collectionId
            return Ok vocabularies
    }

let create env userId collectionId name description now =
    task {
        let! ownershipResult = checkCollectionOwnership env userId collectionId

        match ownershipResult with
        | Error error -> return Error error
        | Ok _ ->
            match validateName name with
            | Error error -> return Error error
            | Ok validName ->
                do! createVocabulary env collectionId validName description now

                let! vocabularies = getVocabulariesByCollectionId env collectionId

                let created =
                    vocabularies
                    |> List.filter(fun v -> v.Name = validName && v.CreatedAt = now)
                    |> List.tryHead

                match created with
                | Some vocabulary -> return Ok vocabulary
                | None -> return Error VocabularyNameRequired
    }

let update env userId vocabularyId name description now =
    task {
        let! maybeVocabulary = getVocabularyById env vocabularyId

        match maybeVocabulary with
        | None -> return Error(VocabularyNotFound vocabularyId)
        | Some vocabulary ->
            let! ownershipResult = checkCollectionOwnership env userId vocabulary.CollectionId

            match ownershipResult with
            | Error _ -> return Error(VocabularyAccessDenied vocabularyId)
            | Ok _ ->
                match validateName name with
                | Error error -> return Error error
                | Ok validName ->
                    let! _ = updateVocabulary env vocabularyId validName description now

                    let updated =
                        { vocabulary with
                            Name = validName
                            Description = description
                            UpdatedAt = Some now }

                    return Ok updated
    }

let delete env userId vocabularyId =
    task {
        let! maybeVocabulary = getVocabularyById env vocabularyId

        match maybeVocabulary with
        | None -> return Error(VocabularyNotFound vocabularyId)
        | Some vocabulary ->
            let! ownershipResult = checkCollectionOwnership env userId vocabulary.CollectionId

            match ownershipResult with
            | Error _ -> return Error(VocabularyAccessDenied vocabularyId)
            | Ok _ ->
                let! _ = deleteVocabulary env vocabularyId
                return Ok()
    }
