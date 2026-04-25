namespace Wordfolio.Api.DataAccess.Tests.ExerciseSessions

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateSessionWithEntriesTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createSessionWithEntriesAsync creates session with no entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 700

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: ExerciseSessions.CreateSessionParameters =
                { UserId = user.Id
                  ExerciseType = 0s
                  Entries = []
                  CreatedAt = createdAt }

            let! sessionId =
                ExerciseSessions.createSessionWithEntriesAsync parameters
                |> fixture.WithConnectionAsync

            let! actual =
                fixture.Seeder
                |> Seeder.getExerciseSessionByIdAsync sessionId

            let expected: ExerciseSession option =
                Some
                    { Id = sessionId
                      UserId = user.Id
                      ExerciseType = 0s
                      CreatedAt = createdAt }

            Assert.Equivalent(expected, actual)

            let! actualEntries =
                fixture.Seeder
                |> Seeder.getExerciseSessionEntriesBySessionIdAsync sessionId

            Assert.Empty(actualEntries)
        }

    [<Fact>]
    member _.``createSessionWithEntriesAsync creates session with entries preserving DisplayOrder and PromptData``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 701

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary "word1" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocabulary "word2" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: ExerciseSessions.CreateSessionParameters =
                { UserId = user.Id
                  ExerciseType = 1s
                  Entries =
                    [ { EntryId = entry1.Id
                        DisplayOrder = 0
                        PromptData = """{"question":"what is word1?"}"""
                        PromptSchemaVersion = 1s }
                      { EntryId = entry2.Id
                        DisplayOrder = 1
                        PromptData = """{"question":"what is word2?"}"""
                        PromptSchemaVersion = 1s } ]
                  CreatedAt = createdAt }

            let! sessionId =
                ExerciseSessions.createSessionWithEntriesAsync parameters
                |> fixture.WithConnectionAsync

            let! actualSession =
                fixture.Seeder
                |> Seeder.getExerciseSessionByIdAsync sessionId

            let expectedSession: ExerciseSession option =
                Some
                    { Id = sessionId
                      UserId = user.Id
                      ExerciseType = 1s
                      CreatedAt = createdAt }

            Assert.Equivalent(expectedSession, actualSession)

            let! actualEntries =
                fixture.Seeder
                |> Seeder.getExerciseSessionEntriesBySessionIdAsync sessionId

            let entry1Row =
                actualEntries
                |> List.find(fun e -> e.EntryId = entry1.Id)

            let entry2Row =
                actualEntries
                |> List.find(fun e -> e.EntryId = entry2.Id)

            let expected: ExerciseSessionEntry list =
                [ { Id = entry1Row.Id
                    SessionId = sessionId
                    EntryId = entry1.Id
                    DisplayOrder = 0
                    PromptData = """{"question":"what is word1?"}"""
                    PromptSchemaVersion = 1s }
                  { Id = entry2Row.Id
                    SessionId = sessionId
                    EntryId = entry2.Id
                    DisplayOrder = 1
                    PromptData = """{"question":"what is word2?"}"""
                    PromptSchemaVersion = 1s } ]

            Assert.Equivalent(expected, actualEntries)
        }

    [<Fact>]
    member _.``createSessionWithEntriesAsync does not affect other sessions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 702

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! sessionId1 =
                ExerciseSessions.createSessionWithEntriesAsync
                    { UserId = user.Id
                      ExerciseType = 0s
                      Entries =
                        [ { EntryId = entry.Id
                            DisplayOrder = 0
                            PromptData = "{}"
                            PromptSchemaVersion = 1s } ]
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! sessionId2 =
                ExerciseSessions.createSessionWithEntriesAsync
                    { UserId = user.Id
                      ExerciseType = 0s
                      Entries = []
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! entriesForSession1 =
                fixture.Seeder
                |> Seeder.getExerciseSessionEntriesBySessionIdAsync sessionId1

            let session1Entry = entriesForSession1[0]

            Assert.Equivalent(
                [ { Id = session1Entry.Id
                    SessionId = sessionId1
                    EntryId = entry.Id
                    DisplayOrder = 0
                    PromptData = "{}"
                    PromptSchemaVersion = 1s } ],
                entriesForSession1
            )

            let! entriesForSession2 =
                fixture.Seeder
                |> Seeder.getExerciseSessionEntriesBySessionIdAsync sessionId2

            Assert.Empty(entriesForSession2)
        }

    [<Fact>]
    member _.``createSessionWithEntriesAsync rolls back session when entry insert fails``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 703

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: ExerciseSessions.CreateSessionParameters =
                { UserId = user.Id
                  ExerciseType = 0s
                  Entries =
                    [ { EntryId = 99999
                        DisplayOrder = 0
                        PromptData = "{}"
                        PromptSchemaVersion = 1s } ]
                  CreatedAt = createdAt }

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (ExerciseSessions.createSessionWithEntriesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)

            let! sessions =
                fixture.Seeder
                |> Seeder.getAllExerciseSessionsAsync

            Assert.Empty(sessions)
        }

    [<Fact>]
    member _.``createSessionWithEntriesAsync fails with foreign key violation for invalid userId``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let parameters: ExerciseSessions.CreateSessionParameters =
                { UserId = 99999
                  ExerciseType = 0s
                  Entries = []
                  CreatedAt = createdAt }

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (ExerciseSessions.createSessionWithEntriesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }
