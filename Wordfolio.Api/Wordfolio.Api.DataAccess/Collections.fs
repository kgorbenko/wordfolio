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
      CreatedAtDateTime: DateTimeOffset
      CreatedAtOffset: int16
      UpdatedAtDateTime: DateTimeOffset
      UpdatedAtOffset: int16 }

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

let private toRecord(collection: Collection) : CollectionRecord =
    { Id = collection.Id
      UserId = collection.UserId
      Name = collection.Name
      Description = collection.Description
      CreatedAtDateTime = collection.CreatedAt
      CreatedAtOffset = int16 collection.CreatedAt.Offset.TotalMinutes
      UpdatedAtDateTime = collection.UpdatedAt
      UpdatedAtOffset = int16 collection.UpdatedAt.Offset.TotalMinutes }

let private fromRecord(record: CollectionRecord) : Collection =
    { Id = record.Id
      UserId = record.UserId
      Name = record.Name
      Description = record.Description
      CreatedAt = DateTimeOffset(record.CreatedAtDateTime.DateTime, TimeSpan.FromMinutes(float record.CreatedAtOffset))
      UpdatedAt = DateTimeOffset(record.UpdatedAtDateTime.DateTime, TimeSpan.FromMinutes(float record.UpdatedAtOffset)) }

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
              CreatedAtDateTime = parameters.CreatedAt
              CreatedAtOffset = int16 parameters.CreatedAt.Offset.TotalMinutes
              UpdatedAtDateTime = parameters.CreatedAt
              UpdatedAtOffset = int16 parameters.CreatedAt.Offset.TotalMinutes }

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
            let updatedRecord =
                { record with
                    Name = parameters.Name
                    Description = parameters.Description
                    UpdatedAtDateTime = parameters.UpdatedAt
                    UpdatedAtOffset = int16 parameters.UpdatedAt.Offset.TotalMinutes }

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
