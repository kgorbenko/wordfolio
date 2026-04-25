namespace Wordfolio.Api.DataAccess.Tests.ExerciseAttempts

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetAttemptsBySessionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getAttemptsBySessionAsync returns empty list when no attempts exist for session``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 750

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getAttemptsBySessionAsync session.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getAttemptsBySessionAsync returns all attempts for the session``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 751

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary "word1" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocabulary "word2" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let attempt1 =
                Entities.makeExerciseAttempt user (Some session) entry1 0s "{}" 1s "ans1" true createdAt

            let attempt2 =
                Entities.makeExerciseAttempt user (Some session) entry2 0s "{}" 1s "ans2" false createdAt

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ attempt1; attempt2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getAttemptsBySessionAsync session.Id
                |> fixture.WithConnectionAsync

            let expected: ExerciseAttempt list =
                [ { Id = attempt1.Id
                    UserId = user.Id
                    SessionId = Some session.Id
                    EntryId = entry1.Id
                    ExerciseType = 0s
                    PromptData = "{}"
                    PromptSchemaVersion = 1s
                    RawAnswer = "ans1"
                    IsCorrect = true
                    AttemptedAt = createdAt }
                  { Id = attempt2.Id
                    UserId = user.Id
                    SessionId = Some session.Id
                    EntryId = entry2.Id
                    ExerciseType = 0s
                    PromptData = "{}"
                    PromptSchemaVersion = 1s
                    RawAnswer = "ans2"
                    IsCorrect = false
                    AttemptedAt = createdAt } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getAttemptsBySessionAsync does not return attempts from other sessions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 752

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

            let attempt =
                Entities.makeExerciseAttempt user (Some session2) entry 0s "{}" 1s "ans" true createdAt

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ attempt ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getAttemptsBySessionAsync session1.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }
