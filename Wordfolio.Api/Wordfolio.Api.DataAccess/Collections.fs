module Wordfolio.Api.DataAccess.Collections

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper
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
      UpdatedAt: Nullable<DateTimeOffset>
      IsSystem: bool }

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

[<CLIMutable>]
type internal CollectionInsertParameters =
    { UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      IsSystem: bool }

let internal collectionsInsertTable =
    table'<CollectionInsertParameters> Schema.CollectionsTable.Name
    |> inSchema Schema.Name

let createCollectionAsync
    (parameters: CollectionCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
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
        let! results =
            select {
                for c in collectionsTable do
                    where(c.UserId = userId && c.IsSystem = false)
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
        let! result =
            select {
                for c in collectionsTable do
                    where(c.UserId = userId && c.IsSystem = true)
            }
            |> selectSingleAsync connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let createDefaultCollectionAsync
    (parameters: CollectionCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
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

type VocabularySummary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CollectionWithVocabularies =
    { Collection: Collection
      Vocabularies: VocabularySummary list }

[<CLIMutable>]
type internal VocabularyRecord =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsDefault: bool }

let getCollectionsWithVocabulariesByUserIdAsync
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<CollectionWithVocabularies list> =
    task {
        let sql =
            """
            SELECT
                c."Id", c."UserId", c."Name", c."Description", c."CreatedAt", c."UpdatedAt", c."IsSystem",
                v."Id", v."CollectionId", v."Name", v."Description", v."CreatedAt", v."UpdatedAt", v."IsDefault"
            FROM wordfolio."Collections" c
            LEFT JOIN wordfolio."Vocabularies" v ON v."CollectionId" = c."Id" AND v."IsDefault" = false
            WHERE c."UserId" = @UserId AND c."IsSystem" = false
            ORDER BY c."Id", v."Id"
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| UserId = userId |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        let! results =
            connection.QueryAsync<CollectionRecord, VocabularyRecord, CollectionRecord * VocabularyRecord option>(
                commandDefinition,
                (fun c v -> (c, Option.ofObj v)),
                splitOn = "Id"
            )

        let grouped =
            results
            |> Seq.groupBy fst
            |> Seq.map(fun (collection, rows) ->
                let vocabularies =
                    rows
                    |> Seq.choose snd
                    |> Seq.map(fun v ->
                        { Id = v.Id
                          CollectionId = v.CollectionId
                          Name = v.Name
                          Description = v.Description
                          CreatedAt = v.CreatedAt
                          UpdatedAt = v.UpdatedAt |> Option.ofNullable })
                    |> Seq.toList

                { Collection = fromRecord collection
                  Vocabularies = vocabularies })
            |> Seq.toList

        return grouped
    }
