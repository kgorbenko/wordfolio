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
      UpdatedAt: Nullable<DateTimeOffset> }

type Collection =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

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
      UpdatedAt =
        if record.UpdatedAt.HasValue then
            Some record.UpdatedAt.Value
        else
            None }

let internal collectionsTable =
    table'<CollectionRecord> Schema.CollectionsTable.Name
    |> inSchema Schema.Name

let internal collectionsInsertTable =
    table'<CollectionCreationParameters> Schema.CollectionsTable.Name
    |> inSchema Schema.Name

let internal collectionsOutputTable =
    table'<CollectionRecord> Schema.CollectionsTable.Name
    |> inSchema Schema.Name

let createCollectionAsync
    (parameters: CollectionCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let query =
            insert {
                into collectionsInsertTable
                values [ parameters ]
            }

        let! records =
            connection.InsertOutputAsync<CollectionCreationParameters, CollectionRecord>(
                query,
                trans = transaction,
                cancellationToken = cancellationToken
            )

        let record = records |> Seq.head
        return record.Id
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
            |> trySelectFirstAsync connection transaction cancellationToken

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
        let! affectedRows =
            update {
                for c in collectionsTable do
                    setColumn c.Name parameters.Name
                    setColumn c.Description parameters.Description
                    setColumn c.UpdatedAt (Nullable parameters.UpdatedAt)
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
