namespace Wordfolio.Api.DataAccess.Tests.ExerciseAttempts

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
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

            match result with
            | ExerciseAttempts.AttemptInserted attemptId ->
                let! attempts =
                    fixture.Seeder
                    |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

                let expected: ExerciseAttempt list =
                    [ { Id = attemptId
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
            | other -> Assert.Fail($"Expected AttemptInserted but got %A{other}")
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

            match result with
            | ExerciseAttempts.IdempotentReplay isCorrect ->
                Assert.True(isCorrect)

                let! attempts =
                    fixture.Seeder
                    |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

                Assert.Equal(1, attempts.Length)
            | other -> Assert.Fail($"Expected IdempotentReplay but got %A{other}")
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

            match result with
            | ExerciseAttempts.ConflictingReplay ->
                let! attempts =
                    fixture.Seeder
                    |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

                Assert.Equal(1, attempts.Length)
                Assert.Equal("answer-one", attempts[0].RawAnswer)
            | other -> Assert.Fail($"Expected ConflictingReplay but got %A{other}")
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

            let! session2Attempts =
                fixture.Seeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session2.Id

            Assert.Empty(session2Attempts)
        }
