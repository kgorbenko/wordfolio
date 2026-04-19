module Wordfolio.Api.Infrastructure.ExerciseSessionPurgeService

open System.Threading
open System.Threading.Tasks

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options

open Wordfolio.Api.Configuration.ExerciseSessionPurge
open Wordfolio.Api.Infrastructure.ExerciseSessionPurgeRunner

type ExerciseSessionPurgeService
    (runner: ExerciseSessionPurgeRunner, options: IOptions<ExerciseSessionPurgeConfiguration>) =
    inherit BackgroundService()

    override _.ExecuteAsync(cancellationToken: CancellationToken) : Task =
        task {
            while not cancellationToken.IsCancellationRequested do
                do! runner.RunOnceAsync(cancellationToken)
                do! Task.Delay(options.Value.Interval, cancellationToken)
        }
