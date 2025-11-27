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
      UpdatedAt: DateTimeOffset }

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

let private fromRecord(record: VocabularyRecord) : Vocabulary =
    { Id = record.Id
      CollectionId = record.CollectionId
      Name = record.Name
      Description = record.Description
      CreatedAt = record.CreatedAt
      UpdatedAt = record.UpdatedAt }

let internal vocabulariesTable =
    table'<VocabularyRecord> Schema.VocabulariesTable.Name
    |> inSchema Schema.Name

type VocabularyCreationParametersWithId =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

let createVocabularyAsync
    (parameters: VocabularyCreationParametersWithId)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<unit> =
    task {
        let vocabularyToInsert: VocabularyRecord =
            { Id = parameters.Id
              CollectionId = parameters.CollectionId
              Name = parameters.Name
              Description = parameters.Description
              CreatedAt = parameters.CreatedAt
              UpdatedAt = parameters.CreatedAt }

        do!
            insert {
                into vocabulariesTable
                values [ vocabularyToInsert ]
            }
            |> insertAsync connection transaction cancellationToken
            |> Task.ignore
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
            let updatedRecord: VocabularyRecord =
                { record with
                    Name = parameters.Name
                    Description = parameters.Description
                    UpdatedAt = parameters.UpdatedAt }

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
