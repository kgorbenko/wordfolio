module Wordfolio.Api.Infrastructure.Environment

open System
open System.Data
open System.Data.Common
open System.Threading
open System.Threading.Tasks

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.DataAccess

type AppEnv(connection: IDbConnection, transaction: IDbTransaction, cancellationToken: CancellationToken) =

    let toCollectionDomain(c: Collections.Collection) : Collection =
        { Id = CollectionId c.Id
          UserId = UserId c.UserId
          Name = c.Name
          Description = c.Description
          CreatedAt = c.CreatedAt
          UpdatedAt = c.UpdatedAt }

    let toVocabularyDomain(v: Vocabularies.Vocabulary) : Vocabulary =
        { Id = VocabularyId v.Id
          CollectionId = CollectionId v.CollectionId
          Name = v.Name
          Description = v.Description
          CreatedAt = v.CreatedAt
          UpdatedAt = v.UpdatedAt }

    interface IGetCollectionById with
        member _.GetCollectionById(CollectionId id) =
            task {
                let! result = Collections.getCollectionByIdAsync id connection transaction cancellationToken
                return result |> Option.map toCollectionDomain
            }

    interface IGetCollectionsByUserId with
        member _.GetCollectionsByUserId(UserId userId) =
            task {
                let! results = Collections.getCollectionsByUserIdAsync userId connection transaction cancellationToken
                return results |> List.map toCollectionDomain
            }

    interface ICreateCollection with
        member _.CreateCollection(UserId userId, name, description, createdAt) =
            task {
                let parameters: Collections.CollectionCreationParameters =
                    { UserId = userId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                do! Collections.createCollectionAsync parameters connection transaction cancellationToken

                let! collections =
                    Collections.getCollectionsByUserIdAsync userId connection transaction cancellationToken

                let created =
                    collections
                    |> List.filter(fun c -> c.Name = name && c.CreatedAt = createdAt)
                    |> List.head

                return toCollectionDomain created
            }

    interface IUpdateCollection with
        member _.UpdateCollection(CollectionId id, name, description, updatedAt) =
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

    interface IDeleteCollection with
        member _.DeleteCollection(CollectionId id) =
            task {
                let! affectedRows = Collections.deleteCollectionAsync id connection transaction cancellationToken
                return affectedRows > 0
            }

    interface IGetVocabularyById with
        member _.GetVocabularyById(VocabularyId id) =
            task {
                let! result = Vocabularies.getVocabularyByIdAsync id connection transaction cancellationToken
                return result |> Option.map toVocabularyDomain
            }

    interface IGetVocabulariesByCollectionId with
        member _.GetVocabulariesByCollectionId(CollectionId collectionId) =
            task {
                let! results =
                    Vocabularies.getVocabulariesByCollectionIdAsync
                        collectionId
                        connection
                        transaction
                        cancellationToken

                return results |> List.map toVocabularyDomain
            }

    interface ICreateVocabulary with
        member _.CreateVocabulary(CollectionId collectionId, name, description, createdAt) =
            task {
                let parameters: Vocabularies.VocabularyCreationParameters =
                    { CollectionId = collectionId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                do! Vocabularies.createVocabularyAsync parameters connection transaction cancellationToken

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

                return toVocabularyDomain created
            }

    interface IUpdateVocabulary with
        member _.UpdateVocabulary(VocabularyId id, name, description, updatedAt) =
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

    interface IDeleteVocabulary with
        member _.DeleteVocabulary(VocabularyId id) =
            task {
                let! affectedRows = Vocabularies.deleteVocabularyAsync id connection transaction cancellationToken
                return affectedRows > 0
            }

type TransactionalEnv(dataSource: NpgsqlDataSource, cancellationToken: CancellationToken) =
    interface ITransactional<AppEnv> with
        member _.RunInTransaction(operation) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)
                use! transaction = connection.BeginTransactionAsync(cancellationToken)

                let env =
                    AppEnv(connection, transaction, cancellationToken)

                let! result = operation env

                match result with
                | Ok _ -> do! transaction.CommitAsync(cancellationToken)
                | Error _ -> do! transaction.RollbackAsync(cancellationToken)

                return result
            }

type NonTransactionalEnv(dataSource: NpgsqlDataSource, cancellationToken: CancellationToken) =
    interface ITransactional<AppEnv> with
        member _.RunInTransaction(operation) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)

                let env =
                    AppEnv(connection, null, cancellationToken)

                return! operation env
            }
