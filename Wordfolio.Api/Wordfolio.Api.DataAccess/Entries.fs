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

type CreateEntryParameters =
    { VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset }

type UpdateEntryParameters =
    { Id: int
      EntryText: string
      UpdatedAt: DateTimeOffset }

type MoveEntryParameters =
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

let createEntryAsync
    (parameters: CreateEntryParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let entriesInsertTable =
            table'<CreateEntryParameters> Schema.EntriesTable.Name
            |> inSchema Schema.Name

        let! record =
            insert {
                into entriesInsertTable
                values [ parameters ]
            }
            |> insertOutputSingleAsync<CreateEntryParameters, EntryRecord> connection transaction cancellationToken

        return record.Id
    }

let getEntryByTextAndVocabularyIdAsync
    (vocabularyId: int)
    (entryText: string)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Entry option> =
    task {
        let entriesTable =
            table'<EntryRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

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
    (parameters: UpdateEntryParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let entriesTable =
            table'<EntryRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

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
    (parameters: MoveEntryParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let entriesTable =
            table'<EntryRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

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
        let entriesTable =
            table'<EntryRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

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

[<CLIMutable>]
type private VocabularyRecord = { Id: int; CollectionId: int }

let hasVocabularyAccessAsync
    (vocabularyId: int)
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<bool> =
    task {
        let vocabulariesTable =
            table'<VocabularyRecord> Schema.VocabulariesTable.Name
            |> inSchema Schema.Name

        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! result =
            select {
                for v in vocabulariesTable do
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)
                    where(v.Id = vocabularyId && c.UserId = userId)
            }
            |> trySelectFirstAsync<VocabularyRecord> connection transaction cancellationToken

        return result |> Option.isSome
    }

let hasVocabularyAccessInCollectionAsync
    (vocabularyId: int)
    (collectionId: int)
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<bool> =
    task {
        let vocabulariesTable =
            table'<VocabularyRecord> Schema.VocabulariesTable.Name
            |> inSchema Schema.Name

        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! result =
            select {
                for v in vocabulariesTable do
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)

                    where(
                        v.Id = vocabularyId
                        && v.CollectionId = collectionId
                        && c.UserId = userId
                    )
            }
            |> trySelectFirstAsync<VocabularyRecord> connection transaction cancellationToken

        return result |> Option.isSome
    }
