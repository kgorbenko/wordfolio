namespace Wordfolio.Api.DataAccess.Tests.ExerciseSessions

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetSessionEntryTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getSessionEntryAsync returns None when session entry does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                ExerciseSessions.getSessionEntryAsync 99999 99999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getSessionEntryAsync returns session entry when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 720

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            let sessionEntry =
                Entities.makeExerciseSessionEntry session entry 0 """{"data":"prompt"}""" 1s

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionEntryAsync session.Id entry.Id
                |> fixture.WithConnectionAsync

            let expected: ExerciseSessionEntry option =
                Some
                    { Id = sessionEntry.Id
                      SessionId = session.Id
                      EntryId = entry.Id
                      DisplayOrder = 0
                      PromptData = """{"data":"prompt"}"""
                      PromptSchemaVersion = 1s }

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getSessionEntryAsync returns None when entryId does not match``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 721

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            Entities.makeExerciseSessionEntry session entry 0 "{}" 1s
            |> ignore

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionEntryAsync session.Id (entry.Id + 1)
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }
