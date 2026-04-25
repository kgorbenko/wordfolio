module Wordfolio.Api.DataAccess.ExerciseAttempts

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper
open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper

[<CLIMutable>]
type internal ExerciseAttemptRecord =
    { Id: int
      UserId: int
      SessionId: int option
      EntryId: int
      ExerciseType: int16
      PromptData: string
      PromptSchemaVersion: int16
      RawAnswer: string
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type ExerciseAttempt =
    { Id: int
      UserId: int
      SessionId: int option
      EntryId: int
      ExerciseType: int16
      PromptData: string
      PromptSchemaVersion: int16
      RawAnswer: string
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type CommitAttemptParameters =
    { UserId: int
      SessionId: int
      EntryId: int
      ExerciseType: int16
      PromptData: string
      PromptSchemaVersion: int16
      RawAnswer: string
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type CommitAttemptResult =
    | AttemptInserted of int
    | IdempotentReplay of bool
    | ConflictingReplay

let private fromRecord(record: ExerciseAttemptRecord) : ExerciseAttempt =
    { Id = record.Id
      UserId = record.UserId
      SessionId = record.SessionId
      EntryId = record.EntryId
      ExerciseType = record.ExerciseType
      PromptData = record.PromptData
      PromptSchemaVersion = record.PromptSchemaVersion
      RawAnswer = record.RawAnswer
      IsCorrect = record.IsCorrect
      AttemptedAt = record.AttemptedAt }

let commitAttemptAsync
    (parameters: CommitAttemptParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<CommitAttemptResult> =
    task {
        let insertSql =
            """
            INSERT INTO wordfolio."ExerciseAttempts"
                ("UserId", "SessionId", "EntryId", "ExerciseType", "PromptData", "PromptSchemaVersion", "RawAnswer", "IsCorrect", "AttemptedAt")
            VALUES
                (@UserId, @SessionId, @EntryId, @ExerciseType, @PromptData, @PromptSchemaVersion, @RawAnswer, @IsCorrect, @AttemptedAt)
            ON CONFLICT ("SessionId", "EntryId") DO NOTHING
            RETURNING "Id";
            """

        let insertCommand =
            CommandDefinition(
                commandText = insertSql,
                parameters =
                    {| UserId = parameters.UserId
                       SessionId = parameters.SessionId
                       EntryId = parameters.EntryId
                       ExerciseType = parameters.ExerciseType
                       PromptData = parameters.PromptData
                       PromptSchemaVersion = parameters.PromptSchemaVersion
                       RawAnswer = parameters.RawAnswer
                       IsCorrect = parameters.IsCorrect
                       AttemptedAt = parameters.AttemptedAt |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        let! insertedId = connection.QueryFirstOrDefaultAsync<Nullable<int>>(insertCommand)

        if insertedId.HasValue then
            return AttemptInserted insertedId.Value
        else
            let readSql =
                """
                SELECT "Id", "UserId", "SessionId", "EntryId", "ExerciseType", "PromptData", "PromptSchemaVersion", "RawAnswer", "IsCorrect", "AttemptedAt"
                FROM wordfolio."ExerciseAttempts"
                WHERE "SessionId" = @sessionId AND "EntryId" = @entryId;
                """

            let readCommand =
                CommandDefinition(
                    commandText = readSql,
                    parameters =
                        {| sessionId = parameters.SessionId
                           entryId = parameters.EntryId |},
                    transaction = transaction,
                    cancellationToken = cancellationToken
                )

            let! existing = connection.QueryFirstOrDefaultAsync<ExerciseAttemptRecord>(readCommand)

            if box existing = null then
                return ConflictingReplay
            else if existing.RawAnswer = parameters.RawAnswer then
                return IdempotentReplay existing.IsCorrect
            else
                return ConflictingReplay
    }

let getAttemptsBySessionAsync
    (sessionId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<ExerciseAttempt list> =
    task {
        let sql =
            """
            SELECT "Id", "UserId", "SessionId", "EntryId", "ExerciseType", "PromptData", "PromptSchemaVersion", "RawAnswer", "IsCorrect", "AttemptedAt"
            FROM wordfolio."ExerciseAttempts"
            WHERE "SessionId" = @sessionId
            ORDER BY "Id";
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| sessionId = sessionId |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        let! results = connection.QueryAsync<ExerciseAttemptRecord>(commandDefinition)

        return
            results
            |> Seq.map fromRecord
            |> Seq.toList
    }

let getWorstKnownEntriesAsync
    (userId: int)
    (scopedEntryIds: int list)
    (count: int)
    (knowledgeWindowSize: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        if scopedEntryIds.IsEmpty then
            return []
        else
            let sql =
                """
                WITH ranked_attempts AS (
                    SELECT "EntryId", "IsCorrect",
                           ROW_NUMBER() OVER (PARTITION BY "EntryId" ORDER BY "AttemptedAt" DESC) AS rn
                    FROM wordfolio."ExerciseAttempts"
                    WHERE "UserId" = @userId
                      AND "EntryId" = ANY(@scopedEntryIds)
                ),
                windowed_scores AS (
                    SELECT "EntryId",
                           SUM(CASE WHEN "IsCorrect" THEN 1.0 ELSE 0.0 END) / COUNT(*) AS hit_rate
                    FROM ranked_attempts
                    WHERE rn <= @knowledgeWindowSize
                    GROUP BY "EntryId"
                )
                SELECT e."Id"
                FROM wordfolio."Entries" e
                LEFT JOIN windowed_scores ws ON ws."EntryId" = e."Id"
                LEFT JOIN (
                    SELECT "EntryId", MAX("AttemptedAt") AS "LastAttemptedAt"
                    FROM wordfolio."ExerciseAttempts"
                    WHERE "UserId" = @userId
                    GROUP BY "EntryId"
                ) last_att ON last_att."EntryId" = e."Id"
                WHERE e."Id" = ANY(@scopedEntryIds)
                ORDER BY
                    COALESCE(ws.hit_rate, 0.0) ASC,
                    last_att."LastAttemptedAt" ASC NULLS FIRST,
                    e."Id" ASC
                LIMIT @count;
                """

            let commandDefinition =
                CommandDefinition(
                    commandText = sql,
                    parameters =
                        {| userId = userId
                           scopedEntryIds = scopedEntryIds |> List.toArray
                           knowledgeWindowSize = knowledgeWindowSize
                           count = count |},
                    transaction = transaction,
                    cancellationToken = cancellationToken
                )

            let! ids = connection.QueryAsync<int>(commandDefinition)
            return ids |> Seq.toList
    }
