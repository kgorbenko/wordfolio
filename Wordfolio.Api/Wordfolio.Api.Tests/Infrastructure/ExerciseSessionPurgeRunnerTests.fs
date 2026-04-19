namespace Wordfolio.Api.Tests.Infrastructure

open System
open System.Threading
open System.Threading.Tasks

open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Extensions.Options
open Npgsql
open Xunit

open Wordfolio.Api.Configuration.ExerciseSessionPurge
open Wordfolio.Api.Infrastructure.ExerciseSessionPurgeRunner
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type ExerciseSessionPurgeRunnerTests(fixture: WordfolioIdentityTestFixture) =
    let makeOptions (enabled: bool) (retentionDays: int) : IOptions<ExerciseSessionPurgeConfiguration> =
        Options.Create(
            { Enabled = enabled
              RetentionPeriod = TimeSpan.FromDays(float retentionDays)
              Interval = TimeSpan.FromHours(1.0) }
        )

    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``RunOnceAsync does nothing when disabled``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            let now = DateTimeOffset.UtcNow
            let expiredAt = now.AddDays(-60.0)

            let user = Entities.makeUser 820

            let expiredSession =
                Entities.makeExerciseSession user 0s expiredAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ expiredSession ]
                |> Seeder.saveChangesAsync

            use dataSource =
                NpgsqlDataSourceBuilder(fixture.ConnectionString).Build()

            let options = makeOptions false 30

            let logger =
                NullLogger<ExerciseSessionPurgeRunner>.Instance

            let runner =
                ExerciseSessionPurgeRunner(options, dataSource, logger)

            do! runner.RunOnceAsync(CancellationToken.None)

            let! allSessions =
                fixture.WordfolioSeeder
                |> Seeder.getAllExerciseSessionsAsync

            Assert.Single(allSessions) |> ignore
        }

    [<Fact>]
    member _.``RunOnceAsync performs full purge against real DB when enabled``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            let now = DateTimeOffset.UtcNow
            let expiredAt = now.AddDays(-60.0)

            let user = Entities.makeUser 821

            let collection =
                Entities.makeCollection user "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "word" now now

            let expiredSession =
                Entities.makeExerciseSession user 0s expiredAt

            let recentSession =
                Entities.makeExerciseSession user 0s now

            let _ =
                Entities.makeExerciseSessionEntry expiredSession entry 0 "{}" 1s

            let _ =
                Entities.makeExerciseSessionEntry recentSession entry 1 "{}" 1s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ expiredSession; recentSession ]
                |> Seeder.saveChangesAsync

            let expiredAttempt =
                Entities.makeExerciseAttempt user (Some expiredSession) entry 0s "{}" 1s "ans1" true now

            let recentAttempt =
                Entities.makeExerciseAttempt user (Some recentSession) entry 0s "{}" 1s "ans2" false now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addExerciseAttempts [ expiredAttempt; recentAttempt ]
                |> Seeder.saveChangesAsync

            use dataSource =
                NpgsqlDataSourceBuilder(fixture.ConnectionString).Build()

            let options = makeOptions true 30

            let logger =
                NullLogger<ExerciseSessionPurgeRunner>.Instance

            let runner =
                ExerciseSessionPurgeRunner(options, dataSource, logger)

            do! runner.RunOnceAsync(CancellationToken.None)

            let! allSessions =
                fixture.WordfolioSeeder
                |> Seeder.getAllExerciseSessionsAsync

            let remainingSession =
                Assert.Single(allSessions)

            Assert.Equal(recentSession.Id, remainingSession.Id)

            let! allSessionEntries =
                fixture.WordfolioSeeder
                |> Seeder.getAllExerciseSessionEntriesAsync

            let remainingEntry =
                Assert.Single(allSessionEntries)

            Assert.Equal(recentSession.Id, remainingEntry.SessionId)

            let! detachedAttempt =
                fixture.WordfolioSeeder
                |> Seeder.getExerciseAttemptByIdAsync expiredAttempt.Id

            Assert.Equal(None, detachedAttempt.Value.SessionId)

            let! preservedAttempt =
                fixture.WordfolioSeeder
                |> Seeder.getExerciseAttemptByIdAsync recentAttempt.Id

            Assert.Equal(Some recentSession.Id, preservedAttempt.Value.SessionId)
        }
