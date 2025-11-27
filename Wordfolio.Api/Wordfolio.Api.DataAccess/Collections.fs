module Wordfolio.Api.DataAccess.Collections

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper
open Wordfolio.Common

[<CLIMutable>]
type internal CollectionRecord =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type Collection =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type CollectionCreationParameters =
    { UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type CollectionUpdateParameters =
    { Id: int
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

let private fromRecord(record: CollectionRecord) : Collection =
    { Id = record.Id
      UserId = record.UserId
      Name = record.Name
      Description = record.Description
      CreatedAt = record.CreatedAt
      UpdatedAt = record.UpdatedAt }

let internal collectionsTable =
    table'<CollectionRecord> Schema.CollectionsTable.Name
    |> inSchema Schema.Name

type CollectionCreationParametersWithId =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

let createCollectionAsync
    (parameters: CollectionCreationParametersWithId)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<unit> =
    task {
        let collectionToInsert: CollectionRecord =
            { Id = parameters.Id
              UserId = parameters.UserId
              Name = parameters.Name
              Description = parameters.Description
              CreatedAt = parameters.CreatedAt
              UpdatedAt = parameters.CreatedAt }

        do!
            insert {
                into collectionsTable
                values [ collectionToInsert ]
            }
            |> insertAsync connection transaction cancellationToken
            |> Task.ignore
    }

let getCollectionByIdAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Collection option> =
    task {
        let! result =
            select {
                for c in collectionsTable do
                    where(c.Id = id)
            }
            |> selectAsyncOption connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let getCollectionsByUserIdAsync
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Collection list> =
    task {
        let! results =
            select {
                for c in collectionsTable do
                    where(c.UserId = userId)
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let updateCollectionAsync
    (parameters: CollectionUpdateParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! existing =
            select {
                for c in collectionsTable do
                    where(c.Id = parameters.Id)
            }
            |> selectAsyncOption connection transaction cancellationToken

        match existing with
        | None -> return 0
        | Some record ->
            let updatedRecord: CollectionRecord =
                { record with
                    Name = parameters.Name
                    Description = parameters.Description
                    UpdatedAt = parameters.UpdatedAt }

            let! affectedRows =
                update {
                    for c in collectionsTable do
                        set updatedRecord
                        where(c.Id = parameters.Id)
                }
                |> updateAsync connection transaction cancellationToken

            return affectedRows
    }

let deleteCollectionAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! affectedRows =
            delete {
                for c in collectionsTable do
                    where(c.Id = id)
            }
            |> deleteAsync connection transaction cancellationToken

        return affectedRows
    }
