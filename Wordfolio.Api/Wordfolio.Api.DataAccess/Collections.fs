module Wordfolio.Api.DataAccess.Collections

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper

[<CLIMutable>]
type internal CollectionRecord =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsSystem: bool }

type Collection =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateCollectionParameters =
    { UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type UpdateCollectionParameters =
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
      UpdatedAt = record.UpdatedAt |> Option.ofNullable }

[<CLIMutable>]
type internal CollectionInsertParameters =
    { UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      IsSystem: bool }

let createCollectionAsync
    (parameters: CreateCollectionParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let collectionsInsertTable =
            table'<CollectionInsertParameters> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let insertParams: CollectionInsertParameters =
            { UserId = parameters.UserId
              Name = parameters.Name
              Description = parameters.Description
              CreatedAt = parameters.CreatedAt
              IsSystem = false }

        let! record =
            insert {
                into collectionsInsertTable
                value insertParams
            }
            |> insertOutputSingleAsync<CollectionInsertParameters, CollectionRecord>
                connection
                transaction
                cancellationToken

        return record.Id
    }

let getCollectionByIdAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Collection option> =
    task {
        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! result =
            select {
                for c in collectionsTable do
                    where(c.Id = id && c.IsSystem = false)
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
        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! results =
            select {
                for c in collectionsTable do
                    where(c.UserId = userId && c.IsSystem = false)
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let updateCollectionAsync
    (parameters: UpdateCollectionParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! affectedRows =
            update {
                for c in collectionsTable do
                    setColumn c.Name parameters.Name
                    setColumn c.Description parameters.Description
                    setColumn c.UpdatedAt (Nullable parameters.UpdatedAt)

                    where(
                        c.Id = parameters.Id
                        && c.IsSystem = false
                    )
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
        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! affectedRows =
            delete {
                for c in collectionsTable do
                    where(c.Id = id && c.IsSystem = false)
            }
            |> deleteAsync connection transaction cancellationToken

        return affectedRows
    }

let getDefaultCollectionByUserIdAsync
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Collection option> =
    task {
        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! result =
            select {
                for c in collectionsTable do
                    where(c.UserId = userId && c.IsSystem = true)
            }
            |> selectSingleAsync connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let createDefaultCollectionAsync
    (parameters: CreateCollectionParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let collectionsInsertTable =
            table'<CollectionInsertParameters> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let insertParams: CollectionInsertParameters =
            { UserId = parameters.UserId
              Name = parameters.Name
              Description = parameters.Description
              CreatedAt = parameters.CreatedAt
              IsSystem = true }

        let! record =
            insert {
                into collectionsInsertTable
                value insertParams
            }
            |> insertOutputSingleAsync<CollectionInsertParameters, CollectionRecord>
                connection
                transaction
                cancellationToken

        return record.Id
    }
