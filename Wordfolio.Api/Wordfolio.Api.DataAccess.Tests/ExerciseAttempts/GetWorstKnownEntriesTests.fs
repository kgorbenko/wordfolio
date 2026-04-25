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
    member _.``getWorstKnownEntriesAsync returns empty list when count is zero``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 766

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

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ entry.Id ] 0 10
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

            Assert.Equal<int list>([ coldEntry.Id; knownEntry.Id ], actual)
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

            let expectedIds =
                [ entry1.Id; entry2.Id; entry3.Id ]
                |> List.sort
                |> List.take 2

            Assert.Equal<int list>(expectedIds, actual)
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

            Assert.Equal<int list>([ badEntry.Id; goodEntry.Id ], actual)
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

            let _ =
                Entities.makeEntry vocabulary "out-of-scope" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ inScopeEntry.Id ] 10 10
                |> fixture.WithConnectionAsync

            Assert.Equal<int list>([ inScopeEntry.Id ], actual)
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync breaks ties by LastAttemptedAt ascending``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let baseAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 767

            let collection =
                Entities.makeCollection user "Collection" None baseAt baseAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None baseAt baseAt false

            let olderEntry =
                Entities.makeEntry vocabulary "older" baseAt baseAt

            let newerEntry =
                Entities.makeEntry vocabulary "newer" baseAt baseAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            // Both entries have 1 correct + 1 wrong = 50% hit rate.
            // olderEntry's last attempt is at -2h, newerEntry's at -1h.
            // With equal hit rates, ORDER BY LastAttemptedAt ASC puts olderEntry first.
            let olderAttempt1 =
                Entities.makeExerciseAttempt user None olderEntry 0s "{}" 1s "a" true (baseAt.AddHours(-3.0))

            let olderAttempt2 =
                Entities.makeExerciseAttempt user None olderEntry 0s "{}" 1s "b" false (baseAt.AddHours(-2.0))

            let newerAttempt1 =
                Entities.makeExerciseAttempt user None newerEntry 0s "{}" 1s "a" true (baseAt.AddHours(-3.0))

            let newerAttempt2 =
                Entities.makeExerciseAttempt user None newerEntry 0s "{}" 1s "b" false (baseAt.AddHours(-1.0))

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ olderAttempt1; olderAttempt2; newerAttempt1; newerAttempt2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ olderEntry.Id; newerEntry.Id ] 10 10
                |> fixture.WithConnectionAsync

            Assert.Equal<int list>([ olderEntry.Id; newerEntry.Id ], actual)
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync breaks ties by Id ascending``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let baseAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 768

            let collection =
                Entities.makeCollection user "Collection" None baseAt baseAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None baseAt baseAt false

            let entry1 =
                Entities.makeEntry vocabulary "entry1" baseAt baseAt

            let entry2 =
                Entities.makeEntry vocabulary "entry2" baseAt baseAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            // Both entries have identical hit rate and LastAttemptedAt; tie-break falls to Id ASC.
            let attempt1 =
                Entities.makeExerciseAttempt user None entry1 0s "{}" 1s "a" false baseAt

            let attempt2 =
                Entities.makeExerciseAttempt user None entry2 0s "{}" 1s "a" false baseAt

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ attempt1; attempt2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ entry1.Id; entry2.Id ] 10 10
                |> fixture.WithConnectionAsync

            Assert.True(entry1.Id < entry2.Id)
            Assert.Equal<int list>([ entry1.Id; entry2.Id ], actual)
        }

    [<Fact>]
    member _.``getWorstKnownEntriesAsync ignores attempts from other users``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let baseAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 769
            let user2 = Entities.makeUser 770

            let collection =
                Entities.makeCollection user1 "Collection" None baseAt baseAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None baseAt baseAt false

            let goodEntry =
                Entities.makeEntry vocabulary "good" baseAt baseAt

            let coldEntry =
                Entities.makeEntry vocabulary "cold" baseAt baseAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.saveChangesAsync

            // user1 has 100% hit rate for goodEntry.
            // user2 has 0% hit rate for goodEntry — must not affect user1's ranking.
            // coldEntry has no attempts from user1.
            let user1GoodAttempt =
                Entities.makeExerciseAttempt user1 None goodEntry 0s "{}" 1s "a" true baseAt

            let user2BadAttempt =
                Entities.makeExerciseAttempt user2 None goodEntry 0s "{}" 1s "a" false baseAt

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ user1GoodAttempt; user2BadAttempt ]
                |> Seeder.saveChangesAsync

            let! actual =
                ExerciseAttempts.getWorstKnownEntriesAsync user1.Id [ goodEntry.Id; coldEntry.Id ] 10 10
                |> fixture.WithConnectionAsync

            Assert.Equal<int list>([ coldEntry.Id; goodEntry.Id ], actual)
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

            let entryA =
                Entities.makeEntry vocabulary "a" baseAt baseAt

            let entryB =
                Entities.makeEntry vocabulary "b" baseAt baseAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            // entryA: false at -3h, true at -2h
            //   window=1: hit_rate = 1.0 (only most recent true)
            //   window=2: hit_rate = 0.5
            // entryB: true at -4h, false at -1h
            //   window=1: hit_rate = 0.0 (only most recent false)
            //   window=2: hit_rate = 0.5
            // With window=1: B (0.0) before A (1.0) → [B, A]
            // With window=2: equal hit rates; A last attempted at -2h, B at -1h → A first → [A, B]
            let attemptA1 =
                Entities.makeExerciseAttempt user None entryA 0s "{}" 1s "a" false (baseAt.AddHours(-3.0))

            let attemptA2 =
                Entities.makeExerciseAttempt user None entryA 0s "{}" 1s "b" true (baseAt.AddHours(-2.0))

            let attemptB1 =
                Entities.makeExerciseAttempt user None entryB 0s "{}" 1s "a" true (baseAt.AddHours(-4.0))

            let attemptB2 =
                Entities.makeExerciseAttempt user None entryB 0s "{}" 1s "b" false (baseAt.AddHours(-1.0))

            do!
                fixture.Seeder
                |> Seeder.addExerciseAttempts [ attemptA1; attemptA2; attemptB1; attemptB2 ]
                |> Seeder.saveChangesAsync

            let! resultWithWindow1 =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ entryA.Id; entryB.Id ] 10 1
                |> fixture.WithConnectionAsync

            let! resultWithWindow2 =
                ExerciseAttempts.getWorstKnownEntriesAsync user.Id [ entryA.Id; entryB.Id ] 10 2
                |> fixture.WithConnectionAsync

            Assert.Equal<int list>([ entryB.Id; entryA.Id ], resultWithWindow1)
            Assert.Equal<int list>([ entryA.Id; entryB.Id ], resultWithWindow2)
        }
