namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsHierarchyTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns collections with their vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection1 =
                Entities.makeCollection user "Collection 1" (Some "Description 1") createdAt None false

            let collection2 =
                Entities.makeCollection user "Collection 2" None createdAt None false

            let vocab1 =
                Entities.makeVocabulary collection1 "Vocab 1" (Some "Vocab desc") createdAt None false

            let vocab2 =
                Entities.makeVocabulary collection1 "Vocab 2" None createdAt None false

            let vocab3 =
                Entities.makeVocabulary collection2 "Vocab 3" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.addVocabularies [ vocab1; vocab2; vocab3 ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionSummary list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Collection 1"
                    Description = Some "Description 1"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    Vocabularies =
                      [ { Id = vocab1.Id
                          Name = "Vocab 1"
                          Description = Some "Vocab desc"
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 0 }
                        { Id = vocab2.Id
                          Name = "Vocab 2"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 0 } ] }
                  { Id = collection2.Id
                    UserId = user.Id
                    Name = "Collection 2"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    Vocabularies =
                      [ { Id = vocab3.Id
                          Name = "Vocab 3"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 0 } ] } ]

            Assert.Equal<CollectionsHierarchy.CollectionSummary list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync filters out system collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            let regularCollection =
                Entities.makeCollection user "Regular" None createdAt None false

            let _ =
                Entities.makeVocabulary systemCollection "Default Vocab" None createdAt None false

            let vocab =
                Entities.makeVocabulary regularCollection "Regular Vocab" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionSummary list =
                [ { Id = regularCollection.Id
                    UserId = user.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    Vocabularies =
                      [ { Id = vocab.Id
                          Name = "Regular Vocab"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 0 } ] } ]

            Assert.Equal<CollectionsHierarchy.CollectionSummary list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync filters out default vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let defaultVocab =
                Entities.makeVocabulary collection "Default" None createdAt None true

            let regularVocab =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocab; regularVocab ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionSummary list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    Vocabularies =
                      [ { Id = regularVocab.Id
                          Name = "Regular"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 0 } ] } ]

            Assert.Equal<CollectionsHierarchy.CollectionSummary list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns empty vocabularies list for collections with no vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Empty Collection" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionSummary list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Empty Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    Vocabularies = [] } ]

            Assert.Equal<CollectionsHierarchy.CollectionSummary list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns correct entry counts``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocab1 =
                Entities.makeVocabulary collection "Vocab 1" None createdAt None false

            let vocab2 =
                Entities.makeVocabulary collection "Vocab 2" None createdAt None false

            let entry1 =
                Entities.makeEntry vocab1 "word1" createdAt None

            let entry2 =
                Entities.makeEntry vocab1 "word2" createdAt None

            let entry3 =
                Entities.makeEntry vocab1 "word3" createdAt None

            let entry4 =
                Entities.makeEntry vocab2 "word4" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocab1; vocab2 ]
                |> Seeder.addEntries [ entry1; entry2; entry3; entry4 ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionSummary list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    Vocabularies =
                      [ { Id = vocab1.Id
                          Name = "Vocab 1"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 3 }
                        { Id = vocab2.Id
                          Name = "Vocab 2"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 1 } ] } ]

            Assert.Equal<CollectionsHierarchy.CollectionSummary list>(expected, actual)
        }

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

            let expected: CollectionsHierarchy.VocabularySummary option =
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

            let expected: CollectionsHierarchy.VocabularySummary option =
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
    member _.``getCollectionsByUserIdAsync does not return collections from other users``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 100
            let user2 = Entities.makeUser 101

            let user1Collection =
                Entities.makeCollection user1 "User 1 Collection" None createdAt None false

            let user2Collection =
                Entities.makeCollection user2 "User 2 Collection" None createdAt None false

            let user1Vocab =
                Entities.makeVocabulary user1Collection "User 1 Vocab" None createdAt None false

            let user2Vocab =
                Entities.makeVocabulary user2Collection "User 2 Vocab" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.addCollections [ user1Collection; user2Collection ]
                |> Seeder.addVocabularies [ user1Vocab; user2Vocab ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsByUserIdAsync user1.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionSummary list =
                [ { Id = user1Collection.Id
                    UserId = user1.Id
                    Name = "User 1 Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    Vocabularies =
                      [ { Id = user1Vocab.Id
                          Name = "User 1 Vocab"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 0 } ] } ]

            Assert.Equal<CollectionsHierarchy.CollectionSummary list>(expected, actual)
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

            let expected: CollectionsHierarchy.VocabularySummary option =
                Some
                    { Id = user1DefaultVocab.Id
                      Name = "My Words"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      EntryCount = 0 }

            Assert.Equal(expected, actual)
        }
