module Wordfolio.Api.DataAccess.Entries

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper
open Wordfolio.Common

[<CLIMutable>]
type internal EntryRecord =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset> }

type Entry =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type EntryCreationParameters =
    { VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset }

type EntryUpdateParameters =
    { Id: int
      EntryText: string
      UpdatedAt: DateTimeOffset }

type EntryMoveParameters =
    { Id: int
      OldVocabularyId: int
      NewVocabularyId: int
      UpdatedAt: DateTimeOffset }

let private fromRecord(record: EntryRecord) : Entry =
    { Id = record.Id
      VocabularyId = record.VocabularyId
      EntryText = record.EntryText
      CreatedAt = record.CreatedAt
      UpdatedAt =
        if record.UpdatedAt.HasValue then
            Some record.UpdatedAt.Value
        else
            None }

let internal entriesTable =
    table'<EntryRecord> Schema.EntriesTable.Name
    |> inSchema Schema.Name

let internal entriesInsertTable =
    table'<EntryCreationParameters> Schema.EntriesTable.Name
    |> inSchema Schema.Name

let createEntryAsync
    (parameters: EntryCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! record =
            insert {
                into entriesInsertTable
                values [ parameters ]
            }
            |> insertOutputSingleAsync<EntryCreationParameters, EntryRecord> connection transaction cancellationToken

        return record.Id
    }

let getEntryByIdAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Entry option> =
    task {
        let! result =
            select {
                for e in entriesTable do
                    where(e.Id = id)
            }
            |> trySelectFirstAsync connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let getEntriesByVocabularyIdAsync
    (vocabularyId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Entry list> =
    task {
        let! results =
            select {
                for e in entriesTable do
                    where(e.VocabularyId = vocabularyId)
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let getEntryByTextAndVocabularyIdAsync
    (vocabularyId: int)
    (entryText: string)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Entry option> =
    task {
        let! result =
            select {
                for e in entriesTable do
                    where(
                        e.VocabularyId = vocabularyId
                        && e.EntryText = entryText
                    )
            }
            |> trySelectFirstAsync connection transaction cancellationToken

        return result |> Option.map fromRecord
    }

let updateEntryAsync
    (parameters: EntryUpdateParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! affectedRows =
            update {
                for e in entriesTable do
                    setColumn e.EntryText parameters.EntryText
                    setColumn e.UpdatedAt (Nullable parameters.UpdatedAt)
                    where(e.Id = parameters.Id)
            }
            |> updateAsync connection transaction cancellationToken

        return affectedRows
    }

let moveEntryAsync
    (parameters: EntryMoveParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! affectedRows =
            update {
                for e in entriesTable do
                    setColumn e.VocabularyId parameters.NewVocabularyId
                    setColumn e.UpdatedAt (Nullable parameters.UpdatedAt)

                    where(
                        e.Id = parameters.Id
                        && e.VocabularyId = parameters.OldVocabularyId
                    )
            }
            |> updateAsync connection transaction cancellationToken

        return affectedRows
    }

let deleteEntryAsync
    (id: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! affectedRows =
            delete {
                for e in entriesTable do
                    where(e.Id = id)
            }
            |> deleteAsync connection transaction cancellationToken

        return affectedRows
    }

[<CLIMutable>]
type private CollectionRecord =
    { Id: int; UserId: int; IsSystem: bool }

let private collectionsTable =
    table'<CollectionRecord> Schema.CollectionsTable.Name
    |> inSchema Schema.Name

let hasVocabularyAccessAsync
    (vocabularyId: int)
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<bool> =
    task {
        let! result =
            (select {
                for v in Vocabularies.vocabulariesTable do
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)
                    where(v.Id = vocabularyId && c.UserId = userId)
             }
             |> trySelectFirstAsync connection transaction cancellationToken)
            : Task<Vocabularies.VocabularyRecord option>

        return result |> Option.isSome
    }
