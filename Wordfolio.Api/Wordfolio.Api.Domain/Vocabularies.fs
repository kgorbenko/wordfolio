module Wordfolio.Api.Domain.Vocabularies

open System
open System.Threading
open System.Threading.Tasks

[<Literal>]
let MaxNameLength = 255

type Vocabulary =
    { Id: VocabularyId
      CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateVocabularyCommand =
    { UserId: UserId
      CollectionId: CollectionId
      Name: string
      Description: string option }

type UpdateVocabularyCommand =
    { UserId: UserId
      VocabularyId: VocabularyId
      Name: string
      Description: string option }

type DeleteVocabularyCommand =
    { UserId: UserId
      VocabularyId: VocabularyId }

let private fromData(data: VocabularyData) : Vocabulary =
    { Id = data.Id
      CollectionId = data.CollectionId
      Name = data.Name
      Description = data.Description
      CreatedAt = data.CreatedAt
      UpdatedAt = data.UpdatedAt }

let private validateName(name: string) : Result<string, VocabularyError> =
    if String.IsNullOrWhiteSpace(name) then
        Error VocabularyNameRequired
    elif name.Length > MaxNameLength then
        Error(VocabularyNameTooLong MaxNameLength)
    else
        Ok(name.Trim())

let private checkCollectionOwnership
    (collectionRepository: ICollectionRepository)
    (userId: UserId)
    (collectionId: CollectionId)
    (cancellationToken: CancellationToken)
    : Task<Result<CollectionData, VocabularyError>> =
    task {
        let! maybeCollection = collectionRepository.GetByIdAsync(collectionId, cancellationToken)

        return
            match maybeCollection with
            | None -> Error(VocabularyCollectionNotFound collectionId)
            | Some collection ->
                if collection.UserId = userId then
                    Ok collection
                else
                    Error(VocabularyCollectionNotFound collectionId)
    }

let getByIdAsync
    (vocabularyRepository: IVocabularyRepository)
    (collectionRepository: ICollectionRepository)
    (userId: UserId)
    (vocabularyId: VocabularyId)
    (cancellationToken: CancellationToken)
    : Task<Result<Vocabulary, VocabularyError>> =
    task {
        let! maybeVocabulary = vocabularyRepository.GetByIdAsync(vocabularyId, cancellationToken)

        match maybeVocabulary with
        | None -> return Error(VocabularyNotFound vocabularyId)
        | Some vocabulary ->
            let! ownershipResult =
                checkCollectionOwnership collectionRepository userId vocabulary.CollectionId cancellationToken

            return
                match ownershipResult with
                | Error _ -> Error(VocabularyAccessDenied vocabularyId)
                | Ok _ -> Ok(fromData vocabulary)
    }

let getByCollectionIdAsync
    (vocabularyRepository: IVocabularyRepository)
    (collectionRepository: ICollectionRepository)
    (userId: UserId)
    (collectionId: CollectionId)
    (cancellationToken: CancellationToken)
    : Task<Result<Vocabulary list, VocabularyError>> =
    task {
        let! ownershipResult =
            checkCollectionOwnership collectionRepository userId collectionId cancellationToken

        match ownershipResult with
        | Error error -> return Error error
        | Ok _ ->
            let! vocabularies = vocabularyRepository.GetByCollectionIdAsync(collectionId, cancellationToken)
            return Ok(vocabularies |> List.map fromData)
    }

let createAsync
    (vocabularyRepository: IVocabularyRepository)
    (collectionRepository: ICollectionRepository)
    (command: CreateVocabularyCommand)
    (now: DateTimeOffset)
    (cancellationToken: CancellationToken)
    : Task<Result<Vocabulary, VocabularyError>> =
    task {
        let! ownershipResult =
            checkCollectionOwnership collectionRepository command.UserId command.CollectionId cancellationToken

        match ownershipResult with
        | Error error -> return Error error
        | Ok _ ->
            match validateName command.Name with
            | Error error -> return Error error
            | Ok validName ->
                let! data =
                    vocabularyRepository.CreateAsync(
                        command.CollectionId,
                        validName,
                        command.Description,
                        now,
                        cancellationToken
                    )

                return Ok(fromData data)
    }

let updateAsync
    (vocabularyRepository: IVocabularyRepository)
    (collectionRepository: ICollectionRepository)
    (command: UpdateVocabularyCommand)
    (now: DateTimeOffset)
    (cancellationToken: CancellationToken)
    : Task<Result<Vocabulary, VocabularyError>> =
    task {
        let! maybeVocabulary = vocabularyRepository.GetByIdAsync(command.VocabularyId, cancellationToken)

        match maybeVocabulary with
        | None -> return Error(VocabularyNotFound command.VocabularyId)
        | Some vocabulary ->
            let! ownershipResult =
                checkCollectionOwnership collectionRepository command.UserId vocabulary.CollectionId cancellationToken

            match ownershipResult with
            | Error _ -> return Error(VocabularyAccessDenied command.VocabularyId)
            | Ok _ ->
                match validateName command.Name with
                | Error error -> return Error error
                | Ok validName ->
                    let! _ =
                        vocabularyRepository.UpdateAsync(
                            command.VocabularyId,
                            validName,
                            command.Description,
                            now,
                            cancellationToken
                        )

                    let updated =
                        { vocabulary with
                            Name = validName
                            Description = command.Description
                            UpdatedAt = Some now }

                    return Ok(fromData updated)
    }

let deleteAsync
    (vocabularyRepository: IVocabularyRepository)
    (collectionRepository: ICollectionRepository)
    (command: DeleteVocabularyCommand)
    (cancellationToken: CancellationToken)
    : Task<Result<unit, VocabularyError>> =
    task {
        let! maybeVocabulary = vocabularyRepository.GetByIdAsync(command.VocabularyId, cancellationToken)

        match maybeVocabulary with
        | None -> return Error(VocabularyNotFound command.VocabularyId)
        | Some vocabulary ->
            let! ownershipResult =
                checkCollectionOwnership collectionRepository command.UserId vocabulary.CollectionId cancellationToken

            match ownershipResult with
            | Error _ -> return Error(VocabularyAccessDenied command.VocabularyId)
            | Ok _ ->
                let! _ = vocabularyRepository.DeleteAsync(command.VocabularyId, cancellationToken)
                return Ok()
    }
