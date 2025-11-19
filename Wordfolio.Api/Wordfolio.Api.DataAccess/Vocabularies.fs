module Wordfolio.Api.DataAccess.Vocabularies

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper
open Wordfolio.Common

type internal VocabularyRecord =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAtDateTime: DateTimeOffset
      CreatedAtOffset: int16
      UpdatedAtDateTime: DateTimeOffset
      UpdatedAtOffset: int16 }

type Vocabulary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

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

let private toRecord(vocabulary: Vocabulary) : VocabularyRecord =
    { Id = vocabulary.Id
      CollectionId = vocabulary.CollectionId
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAtDateTime = vocabulary.CreatedAt
      CreatedAtOffset = int16 vocabulary.CreatedAt.Offset.TotalMinutes
      UpdatedAtDateTime = vocabulary.UpdatedAt
      UpdatedAtOffset = int16 vocabulary.UpdatedAt.Offset.TotalMinutes }

let private fromRecord(record: VocabularyRecord) : Vocabulary =
    { Id = record.Id
      CollectionId = record.CollectionId
      Name = record.Name
      Description = record.Description
      CreatedAt = DateTimeOffset(record.CreatedAtDateTime.DateTime, TimeSpan.FromMinutes(float record.CreatedAtOffset))
      UpdatedAt = DateTimeOffset(record.UpdatedAtDateTime.DateTime, TimeSpan.FromMinutes(float record.UpdatedAtOffset)) }

let internal vocabulariesTable =
    table'<VocabularyRecord> Schema.VocabulariesTable.Name
    |> inSchema Schema.Name

let createVocabularyAsync
    (parameters: VocabularyCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let vocabularyToInsert: VocabularyRecord =
            { Id = 0
              CollectionId = parameters.CollectionId
              Name = parameters.Name
              Description = parameters.Description
              CreatedAtDateTime = parameters.CreatedAt
              CreatedAtOffset = int16 parameters.CreatedAt.Offset.TotalMinutes
              UpdatedAtDateTime = parameters.CreatedAt
              UpdatedAtOffset = int16 parameters.CreatedAt.Offset.TotalMinutes }

        let! insertedId =
            insert {
                into vocabulariesTable
                values [ vocabularyToInsert ]
            }
            |> insertAsync connection transaction cancellationToken

        return insertedId
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
            |> selectAsyncOption connection transaction cancellationToken

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
        let! existing =
            select {
                for v in vocabulariesTable do
                    where(v.Id = parameters.Id)
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
                    for v in vocabulariesTable do
                        set updatedRecord
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
