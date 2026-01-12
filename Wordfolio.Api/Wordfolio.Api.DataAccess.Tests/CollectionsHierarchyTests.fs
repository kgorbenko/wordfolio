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
                          CollectionId = collection1.Id
                          Name = "Vocab 1"
                          Description = Some "Vocab desc"
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 0 }
                        { Id = vocab2.Id
                          CollectionId = collection1.Id
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
                          CollectionId = collection2.Id
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
                          CollectionId = regularCollection.Id
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
                          CollectionId = collection.Id
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
                          CollectionId = collection.Id
                          Name = "Vocab 1"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 3 }
                        { Id = vocab2.Id
                          CollectionId = collection.Id
                          Name = "Vocab 2"
                          Description = None
                          CreatedAt = createdAt
                          UpdatedAt = None
                          EntryCount = 1 } ] } ]

            Assert.Equal<CollectionsHierarchy.CollectionSummary list>(expected, actual)
        }
