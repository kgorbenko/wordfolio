module Wordfolio.Api.Infrastructure.Repositories

open System
open System.Data
open System.Data.Common
open System.Threading
open System.Threading.Tasks

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.DataAccess

let private toCollectionData(collection: Collections.Collection) : CollectionData =
    { Id = CollectionId collection.Id
      UserId = UserId collection.UserId
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt }

let private toVocabularyData(vocabulary: Vocabularies.Vocabulary) : VocabularyData =
    { Id = VocabularyId vocabulary.Id
      CollectionId = CollectionId vocabulary.CollectionId
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt }

type TransactionalCollectionRepository(connection: IDbConnection, transaction: IDbTransaction) =
    interface ICollectionRepository with
        member _.GetByIdAsync(CollectionId id, cancellationToken) =
            task {
                let! result =
                    Collections.getCollectionByIdAsync id connection transaction cancellationToken

                return result |> Option.map toCollectionData
            }

        member _.GetByUserIdAsync(UserId userId, cancellationToken) =
            task {
                let! results =
                    Collections.getCollectionsByUserIdAsync userId connection transaction cancellationToken

                return results |> List.map toCollectionData
            }

        member _.CreateAsync(UserId userId, name, description, createdAt, cancellationToken) =
            task {
                let parameters: Collections.CollectionCreationParameters =
                    { UserId = userId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                do!
                    Collections.createCollectionAsync parameters connection transaction cancellationToken

                let! collections =
                    Collections.getCollectionsByUserIdAsync userId connection transaction cancellationToken

                let created =
                    collections
                    |> List.filter(fun c -> c.Name = name && c.CreatedAt = createdAt)
                    |> List.head

                return toCollectionData created
            }

        member _.UpdateAsync(CollectionId id, name, description, updatedAt, cancellationToken) =
            task {
                let parameters: Collections.CollectionUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                let! affectedRows =
                    Collections.updateCollectionAsync parameters connection transaction cancellationToken

                return affectedRows > 0
            }

        member _.DeleteAsync(CollectionId id, cancellationToken) =
            task {
                let! affectedRows =
                    Collections.deleteCollectionAsync id connection transaction cancellationToken

                return affectedRows > 0
            }

type TransactionalVocabularyRepository(connection: IDbConnection, transaction: IDbTransaction) =
    interface IVocabularyRepository with
        member _.GetByIdAsync(VocabularyId id, cancellationToken) =
            task {
                let! result =
                    Vocabularies.getVocabularyByIdAsync id connection transaction cancellationToken

                return result |> Option.map toVocabularyData
            }

        member _.GetByCollectionIdAsync(CollectionId collectionId, cancellationToken) =
            task {
                let! results =
                    Vocabularies.getVocabulariesByCollectionIdAsync
                        collectionId
                        connection
                        transaction
                        cancellationToken

                return results |> List.map toVocabularyData
            }

        member _.CreateAsync(CollectionId collectionId, name, description, createdAt, cancellationToken) =
            task {
                let parameters: Vocabularies.VocabularyCreationParameters =
                    { CollectionId = collectionId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                do!
                    Vocabularies.createVocabularyAsync parameters connection transaction cancellationToken

                let! vocabularies =
                    Vocabularies.getVocabulariesByCollectionIdAsync
                        collectionId
                        connection
                        transaction
                        cancellationToken

                let created =
                    vocabularies
                    |> List.filter(fun v -> v.Name = name && v.CreatedAt = createdAt)
                    |> List.head

                return toVocabularyData created
            }

        member _.UpdateAsync(VocabularyId id, name, description, updatedAt, cancellationToken) =
            task {
                let parameters: Vocabularies.VocabularyUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                let! affectedRows =
                    Vocabularies.updateVocabularyAsync parameters connection transaction cancellationToken

                return affectedRows > 0
            }

        member _.DeleteAsync(VocabularyId id, cancellationToken) =
            task {
                let! affectedRows =
                    Vocabularies.deleteVocabularyAsync id connection transaction cancellationToken

                return affectedRows > 0
            }

type CollectionRepository(dataSource: NpgsqlDataSource) =
    interface ICollectionRepository with
        member _.GetByIdAsync(CollectionId id, cancellationToken) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! result =
                    Collections.getCollectionByIdAsync id connection null cancellationToken

                return result |> Option.map toCollectionData
            }

        member _.GetByUserIdAsync(UserId userId, cancellationToken) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! results =
                    Collections.getCollectionsByUserIdAsync userId connection null cancellationToken

                return results |> List.map toCollectionData
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

                return toCollectionData created
            }

        member _.UpdateAsync(CollectionId id, name, description, updatedAt, cancellationToken) =
            task {
                let parameters: Collections.CollectionUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! affectedRows =
                    Collections.updateCollectionAsync parameters connection null cancellationToken

                return affectedRows > 0
            }

        member _.DeleteAsync(CollectionId id, cancellationToken) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! affectedRows =
                    Collections.deleteCollectionAsync id connection null cancellationToken

                return affectedRows > 0
            }

type VocabularyRepository(dataSource: NpgsqlDataSource) =
    interface IVocabularyRepository with
        member _.GetByIdAsync(VocabularyId id, cancellationToken) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! result =
                    Vocabularies.getVocabularyByIdAsync id connection null cancellationToken

                return result |> Option.map toVocabularyData
            }

        member _.GetByCollectionIdAsync(CollectionId collectionId, cancellationToken) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! results =
                    Vocabularies.getVocabulariesByCollectionIdAsync
                        collectionId
                        connection
                        null
                        cancellationToken

                return results |> List.map toVocabularyData
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
                    Vocabularies.getVocabulariesByCollectionIdAsync
                        collectionId
                        connection
                        null
                        cancellationToken

                let created =
                    vocabularies
                    |> List.filter(fun v -> v.Name = name && v.CreatedAt = createdAt)
                    |> List.head

                return toVocabularyData created
            }

        member _.UpdateAsync(VocabularyId id, name, description, updatedAt, cancellationToken) =
            task {
                let parameters: Vocabularies.VocabularyUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! affectedRows =
                    Vocabularies.updateVocabularyAsync parameters connection null cancellationToken

                return affectedRows > 0
            }

        member _.DeleteAsync(VocabularyId id, cancellationToken) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let! affectedRows =
                    Vocabularies.deleteVocabularyAsync id connection null cancellationToken

                return affectedRows > 0
            }

type UnitOfWork
    (
        connection: DbConnection,
        transaction: DbTransaction,
        collectionRepository: ICollectionRepository,
        vocabularyRepository: IVocabularyRepository
    ) =
    let mutable disposed = false

    interface IUnitOfWork with
        member _.CollectionRepository = collectionRepository
        member _.VocabularyRepository = vocabularyRepository

        member _.CommitAsync(cancellationToken) =
            transaction.CommitAsync(cancellationToken)

        member _.DisposeAsync() =
            task {
                if not disposed then
                    disposed <- true
                    do! transaction.DisposeAsync()
                    do! connection.DisposeAsync()
            }
            |> ValueTask

type UnitOfWorkFactory(dataSource: NpgsqlDataSource) =
    interface IUnitOfWorkFactory with
        member _.CreateAsync(cancellationToken) =
            task {
                let! connection = dataSource.OpenConnectionAsync(cancellationToken)
                let! transaction = connection.BeginTransactionAsync(cancellationToken)

                let collectionRepository =
                    TransactionalCollectionRepository(connection, transaction)

                let vocabularyRepository =
                    TransactionalVocabularyRepository(connection, transaction)

                return
                    UnitOfWork(connection, transaction, collectionRepository, vocabularyRepository)
                    :> IUnitOfWork
            }
