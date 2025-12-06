module Wordfolio.Api.Infrastructure.Repositories

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.DataAccess

type CollectionRepository(dataSource: NpgsqlDataSource) =
    let withConnection(action: IDbConnection -> IDbTransaction -> CancellationToken -> Task<'a>) =
        fun cancellationToken ->
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)
                return! action connection null cancellationToken
            }

    let toData(collection: Collections.Collection) : CollectionData =
        { Id = CollectionId collection.Id
          UserId = UserId collection.UserId
          Name = collection.Name
          Description = collection.Description
          CreatedAt = collection.CreatedAt
          UpdatedAt = collection.UpdatedAt }

    interface ICollectionRepository with
        member _.GetByIdAsync(CollectionId id, cancellationToken) =
            task {
                let! result =
                    Collections.getCollectionByIdAsync id
                    |> withConnection
                    <| cancellationToken

                return result |> Option.map toData
            }

        member _.GetByUserIdAsync(UserId userId, cancellationToken) =
            task {
                let! results =
                    Collections.getCollectionsByUserIdAsync userId
                    |> withConnection
                    <| cancellationToken

                return results |> List.map toData
            }

        member _.CreateAsync(UserId userId, name, description, createdAt, cancellationToken) =
            task {
                let parameters: Collections.CollectionCreationParameters =
                    { UserId = userId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                do!
                    Collections.createCollectionAsync parameters connection null cancellationToken

                let! collections =
                    Collections.getCollectionsByUserIdAsync userId connection null cancellationToken

                let created =
                    collections
                    |> List.filter(fun c -> c.Name = name && c.CreatedAt = createdAt)
                    |> List.head

                return toData created
            }

        member _.UpdateAsync(CollectionId id, name, description, updatedAt, cancellationToken) =
            task {
                let parameters: Collections.CollectionUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                let! affectedRows =
                    Collections.updateCollectionAsync parameters
                    |> withConnection
                    <| cancellationToken

                return affectedRows > 0
            }

        member _.DeleteAsync(CollectionId id, cancellationToken) =
            task {
                let! affectedRows =
                    Collections.deleteCollectionAsync id
                    |> withConnection
                    <| cancellationToken

                return affectedRows > 0
            }

type VocabularyRepository(dataSource: NpgsqlDataSource) =
    let withConnection(action: IDbConnection -> IDbTransaction -> CancellationToken -> Task<'a>) =
        fun cancellationToken ->
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)
                return! action connection null cancellationToken
            }

    let toData(vocabulary: Vocabularies.Vocabulary) : VocabularyData =
        { Id = VocabularyId vocabulary.Id
          CollectionId = CollectionId vocabulary.CollectionId
          Name = vocabulary.Name
          Description = vocabulary.Description
          CreatedAt = vocabulary.CreatedAt
          UpdatedAt = vocabulary.UpdatedAt }

    interface IVocabularyRepository with
        member _.GetByIdAsync(VocabularyId id, cancellationToken) =
            task {
                let! result =
                    Vocabularies.getVocabularyByIdAsync id
                    |> withConnection
                    <| cancellationToken

                return result |> Option.map toData
            }

        member _.GetByCollectionIdAsync(CollectionId collectionId, cancellationToken) =
            task {
                let! results =
                    Vocabularies.getVocabulariesByCollectionIdAsync collectionId
                    |> withConnection
                    <| cancellationToken

                return results |> List.map toData
            }

        member _.CreateAsync(CollectionId collectionId, name, description, createdAt, cancellationToken) =
            task {
                let parameters: Vocabularies.VocabularyCreationParameters =
                    { CollectionId = collectionId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                do!
                    Vocabularies.createVocabularyAsync parameters connection null cancellationToken

                let! vocabularies =
                    Vocabularies.getVocabulariesByCollectionIdAsync collectionId connection null cancellationToken

                let created =
                    vocabularies
                    |> List.filter(fun v -> v.Name = name && v.CreatedAt = createdAt)
                    |> List.head

                return toData created
            }

        member _.UpdateAsync(VocabularyId id, name, description, updatedAt, cancellationToken) =
            task {
                let parameters: Vocabularies.VocabularyUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                let! affectedRows =
                    Vocabularies.updateVocabularyAsync parameters
                    |> withConnection
                    <| cancellationToken

                return affectedRows > 0
            }

        member _.DeleteAsync(VocabularyId id, cancellationToken) =
            task {
                let! affectedRows =
                    Vocabularies.deleteVocabularyAsync id
                    |> withConnection
                    <| cancellationToken

                return affectedRows > 0
            }
