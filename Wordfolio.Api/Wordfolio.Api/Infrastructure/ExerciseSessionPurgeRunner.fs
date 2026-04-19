module Wordfolio.Api.Infrastructure.ExerciseSessionPurgeRunner

open System
open System.Threading
open System.Threading.Tasks

open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Npgsql

open Wordfolio.Api.Configuration.ExerciseSessionPurge
open Wordfolio.Api.DataAccess

type ExerciseSessionPurgeRunner
    (
        options: IOptions<ExerciseSessionPurgeConfiguration>,
        dataSource: NpgsqlDataSource,
        logger: ILogger<ExerciseSessionPurgeRunner>
    ) =

    member _.RunOnceAsync(cancellationToken: CancellationToken) : Task =
        task {
            let configuration = options.Value

            if configuration.Enabled then
                let cutoff =
                    DateTimeOffset.UtcNow
                    - configuration.RetentionPeriod

                use connection =
                    dataSource.CreateConnection()

                do! connection.OpenAsync(cancellationToken)

                use transaction =
                    connection.BeginTransaction()

                let! detachedCount =
                    ExerciseSessionPurge.detachAttemptsFromExpiredSessionsAsync
                        cutoff
                        connection
                        transaction
                        cancellationToken

                let! deletedCount =
                    ExerciseSessionPurge.deleteExpiredSessionsAsync cutoff connection transaction cancellationToken

                do! transaction.CommitAsync(cancellationToken)

                logger.LogInformation(
                    "Exercise session purge completed: {DetachedCount} attempts detached, {DeletedCount} sessions deleted",
                    detachedCount,
                    deletedCount
                )
        }
