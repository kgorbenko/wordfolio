module Wordfolio.Api.DataAccess.Vocabularies

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper
open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper
open Wordfolio.Common

[<CLIMutable>]
type internal VocabularyRecord =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsDefault: bool }

type Vocabulary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type VocabularyCreationParameters =
    { CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type VocabularyUpdateParameters =
    { Id: int
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

let private fromRecord(record: VocabularyRecord) : Vocabulary =
    { Id = record.Id
      CollectionId = record.CollectionId
      Name = record.Name
      Description = record.Description
      CreatedAt = record.CreatedAt
      UpdatedAt =
        if record.UpdatedAt.HasValue then
            Some record.UpdatedAt.Value
        else
            None }

let internal vocabulariesTable =
    table'<VocabularyRecord> Schema.VocabulariesTable.Name
    |> inSchema Schema.Name

[<CLIMutable>]
type internal VocabularyInsertParameters =
    { CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      IsDefault: bool }

let internal vocabulariesInsertTable =
    table'<VocabularyInsertParameters> Schema.VocabulariesTable.Name
    |> inSchema Schema.Name

[<CLIMutable>]
type internal CollectionRecord =
    { Id: int; UserId: int; IsSystem: bool }

let private collectionsTable =
    table'<CollectionRecord> Schema.CollectionsTable.Name
    |> inSchema Schema.Name

let createVocabularyAsync
    (parameters: VocabularyCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let insertParams: VocabularyInsertParameters =
            { CollectionId = parameters.CollectionId
              Name = parameters.Name
              Description = parameters.Description
              CreatedAt = parameters.CreatedAt
              IsDefault = false }

        let! record =
            insert {
                into vocabulariesInsertTable
                value insertParams
            }
            |> insertOutputSingleAsync<VocabularyInsertParameters, VocabularyRecord>
                connection
                transaction
                cancellationToken

        return record.Id
    }

let getVocabularyByIdAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Vocabulary option> =
    task {
        let! result =
            select {
                for v in vocabulariesTable do
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)

                    where(
                        v.Id = id
                        && v.IsDefault = false
                        && c.IsSystem = false
                    )
            }
            |> trySelectFirstAsync connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let getVocabulariesByCollectionIdAsync
    (collectionId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Vocabulary list> =
    task {
        let! results =
            select {
                for v in vocabulariesTable do
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)

                    where(
                        v.CollectionId = collectionId
                        && v.IsDefault = false
                        && c.IsSystem = false
                    )
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let getVocabularyByIdAndUserIdAsync
    (vocabularyId: int)
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Vocabulary option> =
    task {
        let! result =
            select {
                for v in vocabulariesTable do
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)

                    where(
                        v.Id = vocabularyId
                        && c.UserId = userId
                        && v.IsDefault = false
                        && c.IsSystem = false
                    )
            }
            |> trySelectFirstAsync connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let updateVocabularyAsync
    (parameters: VocabularyUpdateParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let sql =
            """
            UPDATE wordfolio."Vocabularies" v
            SET "Name" = @Name, "Description" = @Description, "UpdatedAt" = @UpdatedAt
            WHERE v."Id" = @Id
              AND v."IsDefault" = false
              AND NOT EXISTS (
                  SELECT 1 FROM wordfolio."Collections" c
                  WHERE c."Id" = v."CollectionId" AND c."IsSystem" = true
              )
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters =
                    {| Id = parameters.Id
                       Name = parameters.Name
                       Description = parameters.Description |> Option.toObj
                       UpdatedAt = Nullable parameters.UpdatedAt |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        return! connection.ExecuteAsync(commandDefinition)
    }

let deleteVocabularyAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let sql =
            """
            DELETE FROM wordfolio."Vocabularies" v
            WHERE v."Id" = @Id
              AND v."IsDefault" = false
              AND NOT EXISTS (
                  SELECT 1 FROM wordfolio."Collections" c
                  WHERE c."Id" = v."CollectionId" AND c."IsSystem" = true
              )
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| Id = id |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        return! connection.ExecuteAsync(commandDefinition)
    }

let getDefaultVocabularyByUserIdAsync
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Vocabulary option> =
    task {
        let! result =
            select {
                for v in vocabulariesTable do
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)
                    where(c.UserId = userId && v.IsDefault = true)
            }
            |> selectSingleAsync connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let createDefaultVocabularyAsync
    (parameters: VocabularyCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let insertParams: VocabularyInsertParameters =
            { CollectionId = parameters.CollectionId
              Name = parameters.Name
              Description = parameters.Description
              CreatedAt = parameters.CreatedAt
              IsDefault = true }

        let! record =
            insert {
                into vocabulariesInsertTable
                value insertParams
            }
            |> insertOutputSingleAsync<VocabularyInsertParameters, VocabularyRecord>
                connection
                transaction
                cancellationToken

        return record.Id
    }
