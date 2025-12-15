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

module DataAccess =
    open Wordfolio.Api.DataAccess

    type Collection = Wordfolio.Api.DataAccess.Collections.Collection
    type CollectionCreationParameters = Wordfolio.Api.DataAccess.Collections.CollectionCreationParameters
    type CollectionUpdateParameters = Wordfolio.Api.DataAccess.Collections.CollectionUpdateParameters
    type Vocabulary = Wordfolio.Api.DataAccess.Vocabularies.Vocabulary
    type VocabularyCreationParameters = Wordfolio.Api.DataAccess.Vocabularies.VocabularyCreationParameters
    type VocabularyUpdateParameters = Wordfolio.Api.DataAccess.Vocabularies.VocabularyUpdateParameters

type AppEnv(connection: IDbConnection, transaction: IDbTransaction, cancellationToken: CancellationToken) =

    let toCollectionDomain(c: DataAccess.Collection) : Collection =
        { Id = CollectionId c.Id
          UserId = UserId c.UserId
          Name = c.Name
          Description = c.Description
          CreatedAt = c.CreatedAt
          UpdatedAt = c.UpdatedAt }

    let toVocabularyDomain(v: DataAccess.Vocabulary) : Vocabulary =
        { Id = VocabularyId v.Id
          CollectionId = CollectionId v.CollectionId
          Name = v.Name
          Description = v.Description
          CreatedAt = v.CreatedAt
          UpdatedAt = v.UpdatedAt }

    interface IGetCollectionById with
        member _.GetCollectionById(CollectionId id) =
            task {
                let! result =
                    Wordfolio.Api.DataAccess.Collections.getCollectionByIdAsync
                        id
                        connection
                        transaction
                        cancellationToken

                return result |> Option.map toCollectionDomain
            }

    interface IGetCollectionsByUserId with
        member _.GetCollectionsByUserId(UserId userId) =
            task {
                let! results =
                    Wordfolio.Api.DataAccess.Collections.getCollectionsByUserIdAsync
                        userId
                        connection
                        transaction
                        cancellationToken

                return results |> List.map toCollectionDomain
            }

    interface ICreateCollection with
        member _.CreateCollection(UserId userId, name, description, createdAt) =
            task {
                let parameters: DataAccess.CollectionCreationParameters =
                    { UserId = userId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                return!
                    Wordfolio.Api.DataAccess.Collections.createCollectionAsync
                        parameters
                        connection
                        transaction
                        cancellationToken
            }

    interface IUpdateCollection with
        member _.UpdateCollection(CollectionId id, name, description, updatedAt) =
            task {
                let parameters: DataAccess.CollectionUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                let! affectedRows =
                    Wordfolio.Api.DataAccess.Collections.updateCollectionAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return affectedRows > 0
            }

    interface IDeleteCollection with
        member _.DeleteCollection(CollectionId id) =
            task {
                let! affectedRows =
                    Wordfolio.Api.DataAccess.Collections.deleteCollectionAsync
                        id
                        connection
                        transaction
                        cancellationToken

                return affectedRows > 0
            }

    interface IGetVocabularyById with
        member _.GetVocabularyById(VocabularyId id) =
            task {
                let! result =
                    Wordfolio.Api.DataAccess.Vocabularies.getVocabularyByIdAsync
                        id
                        connection
                        transaction
                        cancellationToken

                return result |> Option.map toVocabularyDomain
            }

    interface IGetVocabulariesByCollectionId with
        member _.GetVocabulariesByCollectionId(CollectionId collectionId) =
            task {
                let! results =
                    Wordfolio.Api.DataAccess.Vocabularies.getVocabulariesByCollectionIdAsync
                        collectionId
                        connection
                        transaction
                        cancellationToken

                return results |> List.map toVocabularyDomain
            }

    interface ICreateVocabulary with
        member _.CreateVocabulary(CollectionId collectionId, name, description, createdAt) =
            task {
                let parameters: DataAccess.VocabularyCreationParameters =
                    { CollectionId = collectionId
                      Name = name
                      Description = description
                      CreatedAt = createdAt }

                return!
                    Wordfolio.Api.DataAccess.Vocabularies.createVocabularyAsync
                        parameters
                        connection
                        transaction
                        cancellationToken
            }

    interface IUpdateVocabulary with
        member _.UpdateVocabulary(VocabularyId id, name, description, updatedAt) =
            task {
                let parameters: DataAccess.VocabularyUpdateParameters =
                    { Id = id
                      Name = name
                      Description = description
                      UpdatedAt = updatedAt }

                let! affectedRows =
                    Wordfolio.Api.DataAccess.Vocabularies.updateVocabularyAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return affectedRows > 0
            }

    interface IDeleteVocabulary with
        member _.DeleteVocabulary(VocabularyId id) =
            task {
                let! affectedRows =
                    Wordfolio.Api.DataAccess.Vocabularies.deleteVocabularyAsync
                        id
                        connection
                        transaction
                        cancellationToken

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
