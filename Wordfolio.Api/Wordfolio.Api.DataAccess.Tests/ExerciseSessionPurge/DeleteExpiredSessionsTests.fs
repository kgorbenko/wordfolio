namespace Wordfolio.Api.DataAccess.Tests.ExerciseSessionPurge

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type DeleteExpiredSessionsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``deleteExpiredSessionsAsync deletes expired sessions and cascades session entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let now =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let cutoff =
                DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)

            let expiredAt =
                DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 810

            let collection =
                Entities.makeCollection user "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "word" now now

            let expiredSession =
                Entities.makeExerciseSession user 0s expiredAt

            let _ =
                Entities.makeExerciseSessionEntry expiredSession entry 0 "{}" 1s

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ expiredSession ]
                |> Seeder.saveChangesAsync

            let! count =
                ExerciseSessionPurge.deleteExpiredSessionsAsync cutoff
                |> fixture.WithConnectionAsync

            Assert.Equal(1, count)

            let! sessionAfter =
                fixture.Seeder
                |> Seeder.getExerciseSessionByIdAsync expiredSession.Id

            Assert.Equal(None, sessionAfter)

            let! allEntries =
                fixture.Seeder
                |> Seeder.getAllExerciseSessionEntriesAsync

            Assert.Empty(allEntries)
        }

    [<Fact>]
    member _.``deleteExpiredSessionsAsync preserves non-expired sessions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let now =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let cutoff =
                DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)

            let expiredAt =
                DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 811

            let expiredSession =
                Entities.makeExerciseSession user 0s expiredAt

            let recentSession =
                Entities.makeExerciseSession user 0s now

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ expiredSession; recentSession ]
                |> Seeder.saveChangesAsync

            let! count =
                ExerciseSessionPurge.deleteExpiredSessionsAsync cutoff
                |> fixture.WithConnectionAsync

            Assert.Equal(1, count)

            let! allSessions =
                fixture.Seeder
                |> Seeder.getAllExerciseSessionsAsync

            let remaining = Assert.Single(allSessions)
            Assert.Equal(recentSession.Id, remaining.Id)
        }

    [<Fact>]
    member _.``deleteExpiredSessionsAsync returns zero when no sessions are expired``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let now =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let cutoff =
                DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 812

            let recentSession =
                Entities.makeExerciseSession user 0s now

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ recentSession ]
                |> Seeder.saveChangesAsync

            let! count =
                ExerciseSessionPurge.deleteExpiredSessionsAsync cutoff
                |> fixture.WithConnectionAsync

            Assert.Equal(0, count)

            let! allSessions =
                fixture.Seeder
                |> Seeder.getAllExerciseSessionsAsync

            Assert.Single(allSessions) |> ignore
        }
