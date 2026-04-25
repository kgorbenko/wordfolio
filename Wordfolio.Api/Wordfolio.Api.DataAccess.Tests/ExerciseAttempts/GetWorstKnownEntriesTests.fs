namespace Wordfolio.Api.DataAccess.Tests.ExerciseAttempts

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetWorstKnownEntriesTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getWorstKnownEntriesAsync returns empty list when scopedEntryIds is empty``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 760

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [] 10 10
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync includes cold entries with no attempts ranked first``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 761

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let coldEntry =
                Entities.makeEntry vocabulary "cold" createdAt createdAt

            let knownEntry =
                Entities.makeEntry vocabulary "known" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let knownAttempt =
                Entities.makeExerciseAttempt user (Some session) knownEntry 0s "{}" 1s "ans" true createdAt

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ knownAttempt ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ coldEntry.Id; knownEntry.Id ] 10 10
                |> fixture.WithConnectionAsync

            Assert.Equal(2, actual.Length)
            Assert.Equal(coldEntry.Id, actual[0])
            Assert.Equal(knownEntry.Id, actual[1])
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync respects count limit``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 762

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary "e1" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocabulary "e2" createdAt createdAt

            let entry3 =
                Entities.makeEntry vocabulary "e3" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ entry1.Id; entry2.Id; entry3.Id ] 2 10
                |> fixture.WithConnectionAsync

            Assert.Equal(2, actual.Length)
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync orders entries with lower hit rate first``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 763

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let goodEntry =
                Entities.makeEntry vocabulary "good" createdAt createdAt

            let badEntry =
                Entities.makeEntry vocabulary "bad" createdAt createdAt

            let session =
                Entities.makeExerciseSession user 0s createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.saveChangesAsync

            let goodAttempt =
                Entities.makeExerciseAttempt user (Some session) goodEntry 0s "{}" 1s "ans" true createdAt

            let badAttempt =
                Entities.makeExerciseAttempt user (Some session) badEntry 0s "{}" 1s "ans" false createdAt

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ goodAttempt; badAttempt ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ goodEntry.Id; badEntry.Id ] 10 10
                |> fixture.WithConnectionAsync

            Assert.Equal(2, actual.Length)
            Assert.Equal(badEntry.Id, actual[0])
            Assert.Equal(goodEntry.Id, actual[1])
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync only considers entries in scopedEntryIds``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 764

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let inScopeEntry =
                Entities.makeEntry vocabulary "in-scope" createdAt createdAt

            let outOfScopeEntry =
                Entities.makeEntry vocabulary "out-of-scope" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ inScopeEntry.Id ] 10 10
                |> fixture.WithConnectionAsync

            Assert.Equal(1, actual.Length)
            Assert.Equal(inScopeEntry.Id, actual[0])
            Assert.DoesNotContain(outOfScopeEntry.Id, actual)
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync applies knowledge window correctly``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let baseAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 765

            let collection =
                Entities.makeCollection user "Collection" None baseAt baseAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None baseAt baseAt false

            let entry =
                Entities.makeEntry vocabulary "word" baseAt baseAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let attempt1 =
                Entities.makeExerciseAttempt user None entry 0s "{}" 1s "ans1" true (baseAt.AddHours(-2.0))

            let attempt2 =
                Entities.makeExerciseAttempt user None entry 0s "{}" 1s "ans2" false (baseAt.AddHours(-1.0))

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ attempt1; attempt2 ]
                |> Seeder.saveChangesAsync

            let! resultWithWindow1 =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ entry.Id ] 10 1
                |> fixture.WithConnectionAsync

            let! resultWithWindow2 =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ entry.Id ] 10 2
                |> fixture.WithConnectionAsync

            Assert.Equal(1, resultWithWindow1.Length)
            Assert.Equal(1, resultWithWindow2.Length)
        }
