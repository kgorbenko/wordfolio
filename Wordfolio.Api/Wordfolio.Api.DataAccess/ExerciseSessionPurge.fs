module Wordfolio.Api.DataAccess.ExerciseSessionPurge

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper

let detachAttemptsFromExpiredSessionsAsync
    (cutoff: DateTimeOffset)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let sql =
            """
            UPDATE wordfolio."ExerciseAttempts"
            SET "SessionId" = NULL
            WHERE "SessionId" IN (
                SELECT "Id"
                FROM wordfolio."ExerciseSessions"
                WHERE "CreatedAt" < @cutoff
            );
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| cutoff = cutoff |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        return! connection.ExecuteAsync(commandDefinition)
    }

let deleteExpiredSessionsAsync
    (cutoff: DateTimeOffset)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let sql =
            """
            DELETE FROM wordfolio."ExerciseSessions"
            WHERE "CreatedAt" < @cutoff;
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| cutoff = cutoff |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        return! connection.ExecuteAsync(commandDefinition)
    }
