module Wordfolio.Api.DataAccess.Vocabularies

open System
open System.Data
open System.Threading
open System.Threading.Tasks

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
      UpdatedAt: Nullable<DateTimeOffset> }

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

let internal vocabulariesInsertTable =
    table'<VocabularyCreationParameters> Schema.VocabulariesTable.Name
    |> inSchema Schema.Name

let createVocabularyAsync
    (parameters: VocabularyCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! record =
            insert {
                into vocabulariesInsertTable
                values [ parameters ]
            }
            |> insertOutputSingleAsync<VocabularyCreationParameters, VocabularyRecord>
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
                    where(v.Id = id)
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
                    where(v.CollectionId = collectionId)
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let updateVocabularyAsync
    (parameters: VocabularyUpdateParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! affectedRows =
            update {
                for v in vocabulariesTable do
                    setColumn v.Name parameters.Name
                    setColumn v.Description parameters.Description
                    setColumn v.UpdatedAt (Nullable parameters.UpdatedAt)
                    where(v.Id = parameters.Id)
            }
            |> updateAsync connection transaction cancellationToken

        return affectedRows
    }

let deleteVocabularyAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! affectedRows =
            delete {
                for v in vocabulariesTable do
                    where(v.Id = id)
            }
            |> deleteAsync connection transaction cancellationToken

        return affectedRows
    }
