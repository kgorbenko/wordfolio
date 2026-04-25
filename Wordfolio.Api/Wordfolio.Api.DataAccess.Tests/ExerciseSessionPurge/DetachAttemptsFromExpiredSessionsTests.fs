namespace Wordfolio.Api.DataAccess.Tests.ExerciseSessionPurge

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type DetachAttemptsFromExpiredSessionsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``detachAttemptsFromExpiredSessionsAsync detaches attempts from expired sessions while preserving recent session links``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let now =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let cutoff =
                DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)

            let expiredAt =
                DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 800

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

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ expiredSession; recentSession ]
                |> Seeder.saveChangesAsync

            let expiredAttempt =
                Entities.makeExerciseAttempt user (Some expiredSession) entry 0s "{}" 1s "ans1" true now

            let recentAttempt =
                Entities.makeExerciseAttempt user (Some recentSession) entry 0s "{}" 1s "ans2" false now

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ expiredAttempt; recentAttempt ]
                |> Seeder.saveChangesAsync

            let! count =
                ExerciseSessionPurge.detachAttemptsFromExpiredSessionsAsync cutoff
                |> fixture.WithConnectionAsync

            Assert.Equal(1, count)

            let! allAttempts =
                fixture.Seeder
                |> Seeder.getAllExerciseAttemptsAsync

            let detachedAttempt =
                allAttempts
                |> List.find(fun a -> a.Id = expiredAttempt.Id)

            let preservedAttempt =
                allAttempts
                |> List.find(fun a -> a.Id = recentAttempt.Id)

            Assert.Equal(None, detachedAttempt.SessionId)
            Assert.Equal(Some recentSession.Id, preservedAttempt.SessionId)
        }

    [<Fact>]
    member _.``detachAttemptsFromExpiredSessionsAsync returns zero when no sessions are expired``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let now =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let cutoff =
                DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 801

            let collection =
                Entities.makeCollection user "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "word" now now

            let recentSession =
                Entities.makeExerciseSession user 0s now

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ recentSession ]
                |> Seeder.saveChangesAsync

            let attempt =
                Entities.makeExerciseAttempt user (Some recentSession) entry 0s "{}" 1s "ans" true now

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ attempt ]
                |> Seeder.saveChangesAsync

            let! count =
                ExerciseSessionPurge.detachAttemptsFromExpiredSessionsAsync cutoff
                |> fixture.WithConnectionAsync

            Assert.Equal(0, count)

            let! allAttempts =
                fixture.Seeder
                |> Seeder.getAllExerciseAttemptsAsync

            let unchanged =
                allAttempts
                |> List.find(fun a -> a.Id = attempt.Id)

            Assert.Equal(Some recentSession.Id, unchanged.SessionId)
        }

    [<Fact>]
    member _.``detachAttemptsFromExpiredSessionsAsync handles expired sessions with no attempts cleanly``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let now =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let cutoff =
                DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)

            let expiredAt =
                DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 802

            let expiredSession =
                Entities.makeExerciseSession user 0s expiredAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ expiredSession ]
                |> Seeder.saveChangesAsync

            let! count =
                ExerciseSessionPurge.detachAttemptsFromExpiredSessionsAsync cutoff
                |> fixture.WithConnectionAsync

            Assert.Equal(0, count)
        }
