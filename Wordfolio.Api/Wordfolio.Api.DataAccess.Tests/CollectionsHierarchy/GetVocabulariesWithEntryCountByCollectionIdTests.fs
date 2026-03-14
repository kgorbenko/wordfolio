namespace Wordfolio.Api.DataAccess.Tests.CollectionsHierarchy

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsHierarchyGetVocabulariesWithEntryCountByCollectionIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getVocabulariesWithEntryCountByCollectionIdAsync returns correct entry count``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 540

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt None false

            let entry1 =
                Entities.makeEntry vocabulary "word1" createdAt None

            let entry2 =
                Entities.makeEntry vocabulary "word2" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync user.Id collection.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary.Id
                    Name = "Vocabulary"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 2 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``getVocabulariesWithEntryCountByCollectionIdAsync excludes default vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 547

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync user.Id collection.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = regularVocabulary.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``getVocabulariesWithEntryCountByCollectionIdAsync returns empty list for non-existent collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 541

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync user.Id 99999
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
        }

    [<Fact>]
    member _.``getVocabulariesWithEntryCountByCollectionIdAsync returns empty list for another user's collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 542
            let user2 = Entities.makeUser 543

            let user2Collection =
                Entities.makeCollection user2 "User2 Collection" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary user2Collection "Vocab" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.addCollections [ user2Collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync user1.Id user2Collection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
        }

    [<Fact>]
    member _.``getVocabulariesWithEntryCountByCollectionIdAsync returns empty list for system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 544

            let systemCollection =
                Entities.makeCollection user "System" None createdAt None true

            let vocabulary =
                Entities.makeVocabulary systemCollection "Vocab" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync user.Id systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
        }

    [<Fact>]
    member _.``getVocabulariesWithEntryCountByCollectionIdAsync returns empty list when collection has no vocabularies``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 545

            let collection =
                Entities.makeCollection user "Empty Collection" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync user.Id collection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
        }

    [<Fact>]
    member _.``getVocabulariesWithEntryCountByCollectionIdAsync returns vocabularies sorted by UpdatedAt DESC NULLS LAST then Id``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAtEarlier =
                DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero)

            let updatedAtLater =
                DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 546

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabularyWithLateUpdate =
                Entities.makeVocabulary collection "Beta" None createdAt (Some updatedAtLater) false

            let vocabularyWithEarlyUpdate =
                Entities.makeVocabulary collection "Alpha" None createdAt (Some updatedAtEarlier) false

            let vocabularyWithNullUpdate =
                Entities.makeVocabulary collection "Gamma" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies
                    [ vocabularyWithLateUpdate
                      vocabularyWithEarlyUpdate
                      vocabularyWithNullUpdate ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync user.Id collection.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabularyWithLateUpdate.Id
                    Name = "Beta"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = Some updatedAtLater
                    EntryCount = 0 }
                  { Id = vocabularyWithEarlyUpdate.Id
                    Name = "Alpha"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = Some updatedAtEarlier
                    EntryCount = 0 }
                  { Id = vocabularyWithNullUpdate.Id
                    Name = "Gamma"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }
