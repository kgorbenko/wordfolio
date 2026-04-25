namespace Wordfolio.Api.DataAccess.Tests.ExerciseSessions

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetSessionEntriesTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getSessionEntriesAsync returns empty list when session has no entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 730

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionEntriesAsync session.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getSessionEntriesAsync returns all entries for the session ordered by DisplayOrder``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 731

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary "word1" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocabulary "word2" createdAt createdAt

            let entry3 =
                Entities.makeEntry vocabulary "word3" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            let sessionEntry2 =
                Entities.makeExerciseSessionEntry session entry2 1 """{"q":"e2"}""" 1s

            let sessionEntry1 =
                Entities.makeExerciseSessionEntry session entry1 0 """{"q":"e1"}""" 1s

            let sessionEntry3 =
                Entities.makeExerciseSessionEntry session entry3 2 """{"q":"e3"}""" 1s

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionEntriesAsync session.Id
                |> fixture.WithConnectionAsync

            let expected: ExerciseSessionEntry list =
                [ { Id = sessionEntry1.Id
                    SessionId = session.Id
                    EntryId = entry1.Id
                    DisplayOrder = 0
                    PromptData = """{"q":"e1"}"""
                    PromptSchemaVersion = 1s }
                  { Id = sessionEntry2.Id
                    SessionId = session.Id
                    EntryId = entry2.Id
                    DisplayOrder = 1
                    PromptData = """{"q":"e2"}"""
                    PromptSchemaVersion = 1s }
                  { Id = sessionEntry3.Id
                    SessionId = session.Id
                    EntryId = entry3.Id
                    DisplayOrder = 2
                    PromptData = """{"q":"e3"}"""
                    PromptSchemaVersion = 1s } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getSessionEntriesAsync does not return entries from other sessions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 732

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session1 =
                Entities.makeExerciseSession user 0s createdAt

            let session2 =
                Entities.makeExerciseSession user 0s createdAt

            let _ =
                Entities.makeExerciseSessionEntry session2 entry 0 "{}" 1s

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session1; session2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseSessions.getSessionEntriesAsync session1.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }
