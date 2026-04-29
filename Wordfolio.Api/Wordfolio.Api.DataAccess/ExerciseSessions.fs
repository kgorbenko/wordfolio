module Wordfolio.Api.DataAccess.ExerciseSessions

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper
open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper

[<CLIMutable>]
type internal ExerciseSessionRecord =
    { Id: int
      UserId: int
      ExerciseType: int16
      CreatedAt: DateTimeOffset }

type ExerciseSession =
    { Id: int
      UserId: int
      ExerciseType: int16
      CreatedAt: DateTimeOffset }

[<CLIMutable>]
type internal ExerciseSessionEntryRecord =
    { Id: int
      SessionId: int
      EntryId: int
      DisplayOrder: int
      PromptData: string
      PromptSchemaVersion: int16 }

type ExerciseSessionEntry =
    { Id: int
      SessionId: int
      EntryId: int
      DisplayOrder: int
      PromptData: string
      PromptSchemaVersion: int16 }

type CreateSessionEntryParameters =
    { EntryId: int
      DisplayOrder: int
      PromptData: string
      PromptSchemaVersion: int16 }

type CreateSessionParameters =
    { UserId: int
      ExerciseType: int16
      Entries: CreateSessionEntryParameters list
      CreatedAt: DateTimeOffset }

[<CLIMutable>]
type internal SessionInsertParameters =
    { UserId: int
      ExerciseType: int16
      CreatedAt: DateTimeOffset }

[<CLIMutable>]
type internal SessionEntryInsertParameters =
    { SessionId: int
      EntryId: int
      DisplayOrder: int
      PromptData: string
      PromptSchemaVersion: int16 }

let private fromSessionRecord(record: ExerciseSessionRecord) : ExerciseSession =
    { Id = record.Id
      UserId = record.UserId
      ExerciseType = record.ExerciseType
      CreatedAt = record.CreatedAt }

let private fromSessionEntryRecord(record: ExerciseSessionEntryRecord) : ExerciseSessionEntry =
    { Id = record.Id
      SessionId = record.SessionId
      EntryId = record.EntryId
      DisplayOrder = record.DisplayOrder
      PromptData = record.PromptData
      PromptSchemaVersion = record.PromptSchemaVersion }

let createSessionWithEntriesAsync
    (parameters: CreateSessionParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let sessionsInsertTable =
            table'<SessionInsertParameters> Schema.ExerciseSessionsTable.Name
            |> inSchema Schema.Name

        let sessionInsert: SessionInsertParameters =
            { UserId = parameters.UserId
              ExerciseType = parameters.ExerciseType
              CreatedAt = parameters.CreatedAt }

        let! sessionRecord =
            insert {
                into sessionsInsertTable
                values [ sessionInsert ]
            }
            |> insertOutputSingleAsync<SessionInsertParameters, ExerciseSessionRecord>
                connection
                transaction
                cancellationToken

        if not parameters.Entries.IsEmpty then
            let sessionEntriesInsertTable =
                table'<SessionEntryInsertParameters> Schema.ExerciseSessionEntriesTable.Name
                |> inSchema Schema.Name

            let entryInserts =
                parameters.Entries
                |> List.map(fun e ->
                    { SessionId = sessionRecord.Id
                      EntryId = e.EntryId
                      DisplayOrder = e.DisplayOrder
                      PromptData = e.PromptData
                      PromptSchemaVersion = e.PromptSchemaVersion })

            let! _rows =
                insert {
                    into sessionEntriesInsertTable
                    values entryInserts
                }
                |> insertAsync connection transaction cancellationToken

            ignore _rows

        return sessionRecord.Id
    }

let getSessionAsync
    (sessionId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<ExerciseSession option> =
    task {
        let sessionsTable =
            table'<ExerciseSessionRecord> Schema.ExerciseSessionsTable.Name
            |> inSchema Schema.Name

        let! result =
            select {
                for s in sessionsTable do
                    where(s.Id = sessionId)
            }
            |> trySelectFirstAsync connection transaction cancellationToken

        return result |> Option.map fromSessionRecord
    }

let getSessionEntryAsync
    (sessionId: int)
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<ExerciseSessionEntry option> =
    task {
        let sessionEntriesTable =
            table'<ExerciseSessionEntryRecord> Schema.ExerciseSessionEntriesTable.Name
            |> inSchema Schema.Name

        let! result =
            select {
                for e in sessionEntriesTable do
                    where(
                        e.SessionId = sessionId
                        && e.EntryId = entryId
                    )
            }
            |> trySelectFirstAsync connection transaction cancellationToken

        return
            result
            |> Option.map fromSessionEntryRecord
    }

let getSessionEntriesAsync
    (sessionId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<ExerciseSessionEntry list> =
    task {
        let sessionEntriesTable =
            table'<ExerciseSessionEntryRecord> Schema.ExerciseSessionEntriesTable.Name
            |> inSchema Schema.Name

        let! results =
            select {
                for e in sessionEntriesTable do
                    where(e.SessionId = sessionId)
                    orderBy e.DisplayOrder
            }
            |> selectAsync<ExerciseSessionEntryRecord> connection transaction cancellationToken

        return
            results
            |> List.map fromSessionEntryRecord
    }
