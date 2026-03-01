namespace Wordfolio.Api.DataAccess.Tests.CollectionsHierarchy

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsHierarchyGetDefaultVocabularySummaryByUserIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getDefaultVocabularySummaryByUserIdAsync returns default vocabulary with entry count``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            let defaultVocab =
                Entities.makeVocabulary systemCollection "My Words" (Some "Default vocabulary") createdAt None true

            let entry1 =
                Entities.makeEntry defaultVocab "word1" createdAt None

            let entry2 =
                Entities.makeEntry defaultVocab "word2" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocab ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getDefaultVocabularySummaryByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount option =
                Some
                    { Id = defaultVocab.Id
                      Name = "My Words"
                      Description = Some "Default vocabulary"
                      CreatedAt = createdAt
                      UpdatedAt = None
                      EntryCount = 2 }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getDefaultVocabularySummaryByUserIdAsync returns vocabulary with zero entry count when no entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            let defaultVocab =
                Entities.makeVocabulary systemCollection "My Words" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocab ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getDefaultVocabularySummaryByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount option =
                Some
                    { Id = defaultVocab.Id
                      Name = "My Words"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      EntryCount = 0 }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getDefaultVocabularySummaryByUserIdAsync returns None when no default vocabulary exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Regular Collection" None createdAt None false

            let regularVocab =
                Entities.makeVocabulary collection "Regular Vocab" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocab ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getDefaultVocabularySummaryByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getDefaultVocabularySummaryByUserIdAsync does not return default vocabulary from other users``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 100
            let user2 = Entities.makeUser 101

            let user1SystemCollection =
                Entities.makeCollection user1 "Unsorted" None createdAt None true

            let user2SystemCollection =
                Entities.makeCollection user2 "Unsorted" None createdAt None true

            let user1DefaultVocab =
                Entities.makeVocabulary user1SystemCollection "My Words" None createdAt None true

            let user2DefaultVocab =
                Entities.makeVocabulary user2SystemCollection "My Words" None createdAt None true

            let user2Entry =
                Entities.makeEntry user2DefaultVocab "word" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.addCollections [ user1SystemCollection; user2SystemCollection ]
                |> Seeder.addVocabularies [ user1DefaultVocab; user2DefaultVocab ]
                |> Seeder.addEntries [ user2Entry ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getDefaultVocabularySummaryByUserIdAsync user1.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount option =
                Some
                    { Id = user1DefaultVocab.Id
                      Name = "My Words"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      EntryCount = 0 }

            Assert.Equal(expected, actual)
        }
