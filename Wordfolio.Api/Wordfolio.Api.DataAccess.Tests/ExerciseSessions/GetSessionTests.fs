namespace Wordfolio.Api.DataAccess.Tests.ExerciseSessions

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetSessionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getSessionAsync returns None when session does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                ExerciseSessions.getSessionAsync 99999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getSessionAsync returns session when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 710

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionAsync session.Id
                |> fixture.WithConnectionAsync

            let expected: ExerciseSession option =
                Some
                    { Id = session.Id
                      UserId = user.Id
                      ExerciseType = 0s
                      CreatedAt = createdAt }

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getSessionAsync returns session regardless of owning user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 712
            let user2 = Entities.makeUser 713

            let session =
                Entities.makeExerciseSession user1 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionAsync session.Id
                |> fixture.WithConnectionAsync

            let expected: ExerciseSession option =
                Some
                    { Id = session.Id
                      UserId = user1.Id
                      ExerciseType = 0s
                      CreatedAt = createdAt }

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getSessionAsync does not return a different session``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 711

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionAsync(session.Id + 1)
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }
