namespace Wordfolio.Api.DataAccess.Tests.ExerciseAttempts

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CommitAttemptTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``commitAttemptAsync returns AttemptInserted and persists the row on first insert``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 740

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let parameters: ExerciseAttempts.CommitAttemptParameters =
                { UserId = user.Id
                  SessionId = session.Id
                  EntryId = entry.Id
                  ExerciseType = 0s
                  PromptData = """{"choices":["a","b"]}"""
                  PromptSchemaVersion = 1s
                  RawAnswer = "a"
                  IsCorrect = true
                  AttemptedAt = createdAt }

            let! result =
                ExerciseAttempts.commitAttemptAsync parameters
                |> fixture.WithConnectionAsync

            let! attempts =
                fixture.Seeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

            let expected: ExerciseAttempt list =
                [ { Id = attempts[0].Id
                    UserId = user.Id
                    SessionId = Some session.Id
                    EntryId = entry.Id
                    ExerciseType = 0s
                    PromptData = """{"choices":["a","b"]}"""
                    PromptSchemaVersion = 1s
                    RawAnswer = "a"
                    IsCorrect = true
                    AttemptedAt = createdAt } ]

            Assert.Equivalent(expected, attempts)
            Assert.Equal(ExerciseAttempts.AttemptInserted attempts[0].Id, result)
        }

    [<Fact>]
    member _.``commitAttemptAsync returns IdempotentReplay when same answer submitted again``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 741

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let parameters: ExerciseAttempts.CommitAttemptParameters =
                { UserId = user.Id
                  SessionId = session.Id
                  EntryId = entry.Id
                  ExerciseType = 0s
                  PromptData = "{}"
                  PromptSchemaVersion = 1s
                  RawAnswer = "answer"
                  IsCorrect = true
                  AttemptedAt = createdAt }

            let! _ =
                ExerciseAttempts.commitAttemptAsync parameters
                |> fixture.WithConnectionAsync

            let! result =
                ExerciseAttempts.commitAttemptAsync parameters
                |> fixture.WithConnectionAsync

            let! attempts =
                fixture.Seeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

            let expected: ExerciseAttempt list =
                [ { Id = attempts[0].Id
                    UserId = user.Id
                    SessionId = Some session.Id
                    EntryId = entry.Id
                    ExerciseType = 0s
                    PromptData = "{}"
                    PromptSchemaVersion = 1s
                    RawAnswer = "answer"
                    IsCorrect = true
                    AttemptedAt = createdAt } ]

            Assert.Equivalent(expected, attempts)
            Assert.Equal(ExerciseAttempts.IdempotentReplay true, result)
        }

    [<Fact>]
    member _.``commitAttemptAsync returns ConflictingReplay when different answer submitted``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 742

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let firstParameters: ExerciseAttempts.CommitAttemptParameters =
                { UserId = user.Id
                  SessionId = session.Id
                  EntryId = entry.Id
                  ExerciseType = 0s
                  PromptData = "{}"
                  PromptSchemaVersion = 1s
                  RawAnswer = "answer-one"
                  IsCorrect = true
                  AttemptedAt = createdAt }

            let! _ =
                ExerciseAttempts.commitAttemptAsync firstParameters
                |> fixture.WithConnectionAsync

            let secondParameters =
                { firstParameters with
                    RawAnswer = "answer-two" }

            let! result =
                ExerciseAttempts.commitAttemptAsync secondParameters
                |> fixture.WithConnectionAsync

            let! attempts =
                fixture.Seeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

            let expected: ExerciseAttempt list =
                [ { Id = attempts[0].Id
                    UserId = user.Id
                    SessionId = Some session.Id
                    EntryId = entry.Id
                    ExerciseType = 0s
                    PromptData = "{}"
                    PromptSchemaVersion = 1s
                    RawAnswer = "answer-one"
                    IsCorrect = true
                    AttemptedAt = createdAt } ]

            Assert.Equivalent(expected, attempts)
            Assert.Equal(ExerciseAttempts.ConflictingReplay, result)
        }

    [<Fact>]
    member _.``commitAttemptAsync does not affect attempts for other sessions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 743

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

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session1; session2 ]
                |> Seeder.saveChangesAsync

            let! _ =
                ExerciseAttempts.commitAttemptAsync
                    { UserId = user.Id
                      SessionId = session1.Id
                      EntryId = entry.Id
                      ExerciseType = 0s
                      PromptData = "{}"
                      PromptSchemaVersion = 1s
                      RawAnswer = "a"
                      IsCorrect = true
                      AttemptedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! session1Attempts =
                fixture.Seeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session1.Id

            let expected: ExerciseAttempt list =
                [ { Id = session1Attempts[0].Id
                    UserId = user.Id
                    SessionId = Some session1.Id
                    EntryId = entry.Id
                    ExerciseType = 0s
                    PromptData = "{}"
                    PromptSchemaVersion = 1s
                    RawAnswer = "a"
                    IsCorrect = true
                    AttemptedAt = createdAt } ]

            Assert.Equivalent(expected, session1Attempts)

            let! session2Attempts =
                fixture.Seeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session2.Id

            Assert.Empty(session2Attempts)
        }

    [<Fact>]
    member _.``commitAttemptAsync inserts row when existing attempt has null session for same entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 744

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let existingAttempt =
                Entities.makeExerciseAttempt user None entry 0s "{}" 1s "existing" true createdAt

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ existingAttempt ]
                |> Seeder.saveChangesAsync

            let parameters: ExerciseAttempts.CommitAttemptParameters =
                { UserId = user.Id
                  SessionId = session.Id
                  EntryId = entry.Id
                  ExerciseType = 0s
                  PromptData = "{}"
                  PromptSchemaVersion = 1s
                  RawAnswer = "new-answer"
                  IsCorrect = true
                  AttemptedAt = createdAt }

            let! result =
                ExerciseAttempts.commitAttemptAsync parameters
                |> fixture.WithConnectionAsync

            let! allAttempts =
                fixture.Seeder
                |> Seeder.getAllExerciseAttemptsAsync

            let newAttempt =
                allAttempts
                |> List.find(fun a -> a.SessionId = Some session.Id)

            let expected: ExerciseAttempt list =
                [ { Id = existingAttempt.Id
                    UserId = user.Id
                    SessionId = None
                    EntryId = entry.Id
                    ExerciseType = 0s
                    PromptData = "{}"
                    PromptSchemaVersion = 1s
                    RawAnswer = "existing"
                    IsCorrect = true
                    AttemptedAt = createdAt }
                  { Id = newAttempt.Id
                    UserId = user.Id
                    SessionId = Some session.Id
                    EntryId = entry.Id
                    ExerciseType = 0s
                    PromptData = "{}"
                    PromptSchemaVersion = 1s
                    RawAnswer = "new-answer"
                    IsCorrect = true
                    AttemptedAt = createdAt } ]

            Assert.Equivalent(expected, allAttempts)
            Assert.Equal(ExerciseAttempts.AttemptInserted newAttempt.Id, result)
        }

    [<Fact>]
    member _.``commitAttemptAsync fails with foreign key violation for invalid userId``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 745

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (ExerciseAttempts.commitAttemptAsync
                        { UserId = 99999
                          SessionId = session.Id
                          EntryId = entry.Id
                          ExerciseType = 0s
                          PromptData = "{}"
                          PromptSchemaVersion = 1s
                          RawAnswer = "a"
                          IsCorrect = true
                          AttemptedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``commitAttemptAsync fails with foreign key violation for invalid entryId``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 746

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (ExerciseAttempts.commitAttemptAsync
                        { UserId = user.Id
                          SessionId = session.Id
                          EntryId = 99999
                          ExerciseType = 0s
                          PromptData = "{}"
                          PromptSchemaVersion = 1s
                          RawAnswer = "a"
                          IsCorrect = true
                          AttemptedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }
