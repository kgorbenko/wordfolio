module Wordfolio.Api.DataAccess.Entries

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper
open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper
open Wordfolio.Common

[<CLIMutable>]
type internal EntryRecord =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type Entry =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type CreateEntryParameters =
    { VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset }

[<CLIMutable>]
type internal EntryInsertParameters =
    { VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

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
      UpdatedAt = record.UpdatedAt }

let createEntryAsync
    (parameters: CreateEntryParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let entriesInsertTable =
            table'<EntryInsertParameters> Schema.EntriesTable.Name
            |> inSchema Schema.Name

        let insertParameters: EntryInsertParameters =
            { VocabularyId = parameters.VocabularyId
              EntryText = parameters.EntryText
              CreatedAt = parameters.CreatedAt
              UpdatedAt = parameters.CreatedAt }

        let! record =
            insert {
                into entriesInsertTable
                values [ insertParameters ]
            }
            |> insertOutputSingleAsync<EntryInsertParameters, EntryRecord> connection transaction cancellationToken

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
                    setColumn e.UpdatedAt parameters.UpdatedAt
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
                    setColumn e.UpdatedAt parameters.UpdatedAt

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
type private EntryIdRecord = { Id: int; VocabularyId: int }

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

let getEntryIdsByVocabularyIdAsync
    (vocabularyId: int)
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        let entriesTable =
            table'<EntryIdRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

        let vocabulariesTable =
            table'<VocabularyRecord> Schema.VocabulariesTable.Name
            |> inSchema Schema.Name

        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! results =
            select {
                for e in entriesTable do
                    innerJoin v in vocabulariesTable on (e.VocabularyId = v.Id)
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)

                    where(
                        e.VocabularyId = vocabularyId
                        && c.UserId = userId
                    )
            }
            |> selectAsync<EntryIdRecord> connection transaction cancellationToken

        return results |> List.map(fun r -> r.Id)
    }

let getEntryIdsByCollectionIdAsync
    (collectionId: int)
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        let entriesTable =
            table'<EntryIdRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

        let vocabulariesTable =
            table'<VocabularyRecord> Schema.VocabulariesTable.Name
            |> inSchema Schema.Name

        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! results =
            select {
                for e in entriesTable do
                    innerJoin v in vocabulariesTable on (e.VocabularyId = v.Id)
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)

                    where(
                        v.CollectionId = collectionId
                        && c.UserId = userId
                    )
            }
            |> selectAsync<EntryIdRecord> connection transaction cancellationToken

        return results |> List.map(fun r -> r.Id)
    }

let getEntryIdsByUserIdAsync
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        let entriesTable =
            table'<EntryIdRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

        let vocabulariesTable =
            table'<VocabularyRecord> Schema.VocabulariesTable.Name
            |> inSchema Schema.Name

        let collectionsTable =
            table'<CollectionRecord> Schema.CollectionsTable.Name
            |> inSchema Schema.Name

        let! results =
            select {
                for e in entriesTable do
                    innerJoin v in vocabulariesTable on (e.VocabularyId = v.Id)
                    innerJoin c in collectionsTable on (v.CollectionId = c.Id)
                    where(c.UserId = userId)
            }
            |> selectAsync<EntryIdRecord> connection transaction cancellationToken

        return results |> List.map(fun r -> r.Id)
    }

let getEntryIdsByIdsForUserAsync
    (requestedIds: int list)
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        if requestedIds.IsEmpty then
            return []
        else
            let entriesTable =
                table'<EntryIdRecord> Schema.EntriesTable.Name
                |> inSchema Schema.Name

            let vocabulariesTable =
                table'<VocabularyRecord> Schema.VocabulariesTable.Name
                |> inSchema Schema.Name

            let collectionsTable =
                table'<CollectionRecord> Schema.CollectionsTable.Name
                |> inSchema Schema.Name

            let! results =
                select {
                    for e in entriesTable do
                        innerJoin v in vocabulariesTable on (e.VocabularyId = v.Id)
                        innerJoin c in collectionsTable on (v.CollectionId = c.Id)

                        where(
                            isIn e.Id requestedIds
                            && c.UserId = userId
                        )
                }
                |> selectAsync<EntryIdRecord> connection transaction cancellationToken

            return results |> List.map(fun r -> r.Id)
    }
