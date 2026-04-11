namespace Wordfolio.Api.DataAccess.Tests.CollectionsHierarchy

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetCollectionsWithVocabularyCountByUserIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getCollectionsWithVocabularyCountByUserIdAsync returns collections with correct vocabulary counts``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 550

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocab1 =
                Entities.makeVocabulary collection "Vocab 1" None createdAt createdAt false

            let vocab2 =
                Entities.makeVocabulary collection "Vocab 2" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocab1; vocab2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsWithVocabularyCountByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    VocabularyCount = 2 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsWithVocabularyCountByUserIdAsync excludes system collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 551

            let regularCollection =
                Entities.makeCollection user "Regular" None createdAt createdAt false

            let systemCollection =
                Entities.makeCollection user "System" None createdAt createdAt true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ regularCollection; systemCollection ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsWithVocabularyCountByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = regularCollection.Id
                    UserId = user.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsWithVocabularyCountByUserIdAsync excludes default vocabularies from count``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 552

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular Vocab" None createdAt createdAt false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default Vocab" None createdAt createdAt true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsWithVocabularyCountByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    VocabularyCount = 1 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsWithVocabularyCountByUserIdAsync returns empty list when no collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 553

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsWithVocabularyCountByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>([], actual)
        }

    [<Fact>]
    member _.``getCollectionsWithVocabularyCountByUserIdAsync does not return other users' collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 554
            let user2 = Entities.makeUser 555

            let user1Collection =
                Entities.makeCollection user1 "User 1 Collection" None createdAt createdAt false

            let user2Collection =
                Entities.makeCollection user2 "User 2 Collection" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.addCollections [ user1Collection; user2Collection ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsWithVocabularyCountByUserIdAsync user1.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = user1Collection.Id
                    UserId = user1.Id
                    Name = "User 1 Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsWithVocabularyCountByUserIdAsync returns zero count for collections with no vocabularies``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 556

            let collection =
                Entities.makeCollection user "Empty Collection" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsWithVocabularyCountByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Empty Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsWithVocabularyCountByUserIdAsync returns collections sorted by UpdatedAt desc nulls last then Id``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt1 =
                DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero)

            let updatedAt2 =
                DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 557

            let collectionA =
                Entities.makeCollection user "A" None createdAt updatedAt1 false

            let collectionB =
                Entities.makeCollection user "B" None createdAt updatedAt2 false

            let collectionC =
                Entities.makeCollection user "C" None createdAt createdAt false

            let collectionD =
                Entities.makeCollection user "D" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collectionA; collectionB; collectionC; collectionD ]
                |> Seeder.saveChangesAsync

            let! actual =
                CollectionsHierarchy.getCollectionsWithVocabularyCountByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collectionA.Id
                    UserId = user.Id
                    Name = "A"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = updatedAt1
                    VocabularyCount = 0 }
                  { Id = collectionB.Id
                    UserId = user.Id
                    Name = "B"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = updatedAt2
                    VocabularyCount = 0 }
                  { Id = collectionC.Id
                    UserId = user.Id
                    Name = "C"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    VocabularyCount = 0 }
                  { Id = collectionD.Id
                    UserId = user.Id
                    Name = "D"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
        }
