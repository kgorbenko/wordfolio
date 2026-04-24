module Wordfolio.Api.Infrastructure.ExerciseSessionPurgeService

open System
open System.Threading
open System.Threading.Tasks

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

open Wordfolio.Api.Configuration.ExerciseSessionPurge
open Wordfolio.Api.Infrastructure.ExerciseSessionPurgeRunner

type ExerciseSessionPurgeService
    (
        runner: ExerciseSessionPurgeRunner,
        options: IOptions<ExerciseSessionPurgeConfiguration>,
        logger: ILogger<ExerciseSessionPurgeService>
    ) =
    inherit BackgroundService()

    override _.ExecuteAsync(cancellationToken: CancellationToken) : Task =
        task {
            while not cancellationToken.IsCancellationRequested do
                try
                    do! runner.RunOnceAsync(cancellationToken)
                with
                | :? OperationCanceledException -> ()
                | ex -> logger.LogError(ex, "Exercise session purge tick failed; will retry after interval")

                do! Task.Delay(options.Value.Interval, cancellationToken)
        }
