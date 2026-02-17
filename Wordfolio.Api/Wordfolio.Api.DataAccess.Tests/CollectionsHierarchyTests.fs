namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsHierarchyTests(fixture: WordfolioTestFixture) =
    let seedCollectionsForSortingTests() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt1 =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let createdAt2 =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let createdAt3 =
                DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero)

            let updatedAt2 =
                DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero)

            let updatedAt3 =
                DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 502

            let alpha =
                Entities.makeCollection user "Alpha" None createdAt1 None false

            let beta =
                Entities.makeCollection user "Beta" None createdAt2 (Some updatedAt2) false

            let gamma =
                Entities.makeCollection user "Gamma" None createdAt3 (Some updatedAt3) false

            let alphaVocab1 =
                Entities.makeVocabulary alpha "Alpha 1" None createdAt1 None false

            let alphaVocab2 =
                Entities.makeVocabulary alpha "Alpha 2" None createdAt1 None false

            let betaVocab =
                Entities.makeVocabulary beta "Beta 1" None createdAt2 None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ alpha; beta; gamma ]
                |> Seeder.addVocabularies [ alphaVocab1; alphaVocab2; betaVocab ]
                |> Seeder.saveChangesAsync

            let alphaSummary: CollectionsHierarchy.CollectionOverview =
                { Id = alpha.Id
                  UserId = user.Id
                  Name = "Alpha"
                  Description = None
                  CreatedAt = createdAt1
                  UpdatedAt = None
                  VocabularyCount = 2 }

            let betaSummary: CollectionsHierarchy.CollectionOverview =
                { Id = beta.Id
                  UserId = user.Id
                  Name = "Beta"
                  Description = None
                  CreatedAt = createdAt2
                  UpdatedAt = Some updatedAt2
                  VocabularyCount = 1 }

            let gammaSummary: CollectionsHierarchy.CollectionOverview =
                { Id = gamma.Id
                  UserId = user.Id
                  Name = "Gamma"
                  Description = None
                  CreatedAt = createdAt3
                  UpdatedAt = Some updatedAt3
                  VocabularyCount = 0 }

            return user.Id, alphaSummary, betaSummary, gammaSummary
        }

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

    [<Fact>]
    member _.``searchUserCollectionsAsync filters by search and sorts by updatedAt descending``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAtA =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let createdAtB =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let updatedAtA =
                DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero)

            let updatedAtB =
                DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 500

            let collectionA =
                Entities.makeCollection user "Biology" (Some "Words for school") createdAtA (Some updatedAtA) false

            let collectionB =
                Entities.makeCollection user "Travel" (Some "Bio terms") createdAtB (Some updatedAtB) false

            let collectionC =
                Entities.makeCollection user "Sports" None createdAtB None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collectionA; collectionB; collectionC ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "bio"
                  SortBy = CollectionsHierarchy.CollectionSortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collectionB.Id
                    UserId = user.Id
                    Name = "Travel"
                    Description = Some "Bio terms"
                    CreatedAt = createdAtB
                    UpdatedAt = Some updatedAtB
                    VocabularyCount = 0 }
                  { Id = collectionA.Id
                    UserId = user.Id
                    Name = "Biology"
                    Description = Some "Words for school"
                    CreatedAt = createdAtA
                    UpdatedAt = Some updatedAtA
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync excludes system collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 501

            let regularCollection =
                Entities.makeCollection user "Regular" None createdAt None false

            let systemCollection =
                Entities.makeCollection user "System" None createdAt None true

            let regularVocabulary =
                Entities.makeVocabulary regularCollection "Regular Vocab" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ regularCollection; systemCollection ]
                |> Seeder.addVocabularies [ regularVocabulary ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = regularCollection.Id
                    UserId = user.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 1 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync excludes default vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 502

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular Vocab" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default Vocab" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 1 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by name ascending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by name descending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by createdAt ascending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.CreatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by createdAt descending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.CreatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by updatedAt ascending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by updatedAt descending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by vocabularyCount ascending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.VocabularyCount
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync sorts by vocabularyCount descending``() =
        task {
            let! userId, alpha, beta, gamma = seedCollectionsForSortingTests()

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.VocabularyCount
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync userId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync performs case-insensitive search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 507

            let collection =
                Entities.makeCollection user "Biology" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "BIO"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Biology"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync returns empty list when search matches nothing``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 508

            let collection =
                Entities.makeCollection user "Biology" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "xyz"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>([], actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync escapes percent wildcard in search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 514

            let collection1 =
                Entities.makeCollection user "100% Complete" None createdAt None false

            let collection2 =
                Entities.makeCollection user "100 Tasks" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "100%"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "100% Complete"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync escapes underscore wildcard in search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 515

            let collection1 =
                Entities.makeCollection user "user_name" None createdAt None false

            let collection2 =
                Entities.makeCollection user "username" None createdAt None false

            let collection3 =
                Entities.makeCollection user "userXname" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2; collection3 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "user_name"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "user_name"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync escapes backslash in search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 516

            let collection1 =
                Entities.makeCollection user @"C:\Users" None createdAt None false

            let collection2 =
                Entities.makeCollection user "C:/Users" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some @"C:\Users"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = @"C:\Users"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync escapes wildcards in description search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 517

            let collection1 =
                Entities.makeCollection user "Collection1" (Some "100% accurate") createdAt None false

            let collection2 =
                Entities.makeCollection user "Collection2" (Some "100 percent") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "100%"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Collection1"
                    Description = Some "100% accurate"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync escapes underscore wildcard in description search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 518

            let collection1 =
                Entities.makeCollection user "Collection1" (Some "user_name format") createdAt None false

            let collection2 =
                Entities.makeCollection user "Collection2" (Some "userXname format") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "user_name"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Collection1"
                    Description = Some "user_name format"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync escapes backslash in description search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 519

            let collection1 =
                Entities.makeCollection user "Collection1" (Some @"Path: C:\Users") createdAt None false

            let collection2 =
                Entities.makeCollection user "Collection2" (Some "Path: C:/Users") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some @"C:\Users"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Collection1"
                    Description = Some @"Path: C:\Users"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync returns correct vocabulary count``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 509

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
                Entities.makeEntry vocab2 "word3" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocab1; vocab2 ]
                |> Seeder.addEntries [ entry1; entry2; entry3 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 2 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync does not return collections from other users``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 510
            let user2 = Entities.makeUser 511

            let user1Collection =
                Entities.makeCollection user1 "User 1 Collection" None createdAt None false

            let user2Collection =
                Entities.makeCollection user2 "User 2 Collection" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.addCollections [ user1Collection; user2Collection ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user1.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = user1Collection.Id
                    UserId = user1.Id
                    Name = "User 1 Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    member private _.SeedVocabulariesForSortingTests() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt1 =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let createdAt2 =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let createdAt3 =
                DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero)

            let updatedAt1 =
                DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero)

            let updatedAt2 =
                DateTimeOffset(2025, 1, 8, 0, 0, 0, TimeSpan.Zero)

            let updatedAt3 =
                DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 520

            let collection =
                Entities.makeCollection user "Collection" None createdAt1 None false

            let alpha =
                Entities.makeVocabulary collection "Alpha" None createdAt1 (Some updatedAt1) false

            let beta =
                Entities.makeVocabulary collection "Beta" None createdAt2 (Some updatedAt2) false

            let gamma =
                Entities.makeVocabulary collection "Gamma" None createdAt3 (Some updatedAt3) false

            let alphaEntry1 =
                Entities.makeEntry alpha "alpha1" createdAt1 None

            let alphaEntry2 =
                Entities.makeEntry alpha "alpha2" createdAt1 None

            let betaEntry1 =
                Entities.makeEntry beta "beta1" createdAt2 None

            let gammaEntry1 =
                Entities.makeEntry gamma "gamma1" createdAt3 None

            let gammaEntry2 =
                Entities.makeEntry gamma "gamma2" createdAt3 None

            let gammaEntry3 =
                Entities.makeEntry gamma "gamma3" createdAt3 None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ alpha; beta; gamma ]
                |> Seeder.addEntries [ alphaEntry1; alphaEntry2; betaEntry1; gammaEntry1; gammaEntry2; gammaEntry3 ]
                |> Seeder.saveChangesAsync

            let alphaSummary: CollectionsHierarchy.VocabularySummary =
                { Id = alpha.Id
                  Name = "Alpha"
                  Description = None
                  CreatedAt = createdAt1
                  UpdatedAt = Some updatedAt1
                  EntryCount = 2 }

            let betaSummary: CollectionsHierarchy.VocabularySummary =
                { Id = beta.Id
                  Name = "Beta"
                  Description = None
                  CreatedAt = createdAt2
                  UpdatedAt = Some updatedAt2
                  EntryCount = 1 }

            let gammaSummary: CollectionsHierarchy.VocabularySummary =
                { Id = gamma.Id
                  Name = "Gamma"
                  Description = None
                  CreatedAt = createdAt3
                  UpdatedAt = Some updatedAt3
                  EntryCount = 3 }

            return user.Id, collection.Id, alphaSummary, betaSummary, gammaSummary
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync returns vocabularies with entry counts``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 521

            let collection =
                Entities.makeCollection user "Collection" (Some "Description") createdAt None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default" None createdAt None true

            let entry1 =
                Entities.makeEntry regularVocabulary "word1" createdAt None

            let entry2 =
                Entities.makeEntry regularVocabulary "word2" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = regularVocabulary.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 2 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync returns empty list for non-existent collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 522

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id 99999 query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>([], actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync returns empty list for other user's collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 523
            let user2 = Entities.makeUser 524

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

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user1.Id user2Collection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>([], actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync returns empty list for system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 525

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

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id systemCollection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>([], actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync returns empty list when collection has no vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 526

            let collection =
                Entities.makeCollection user "Empty Collection" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>([], actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync performs case-insensitive search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 527

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Biology Terms" (Some "Science vocab") createdAt None false

            let otherVocabulary =
                Entities.makeVocabulary collection "Travel Words" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary; otherVocabulary ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some "BIO"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary.Id
                    Name = "Biology Terms"
                    Description = Some "Science vocab"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by name ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by name descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by createdAt ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.CreatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by createdAt descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.CreatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by updatedAt ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by updatedAt descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by entryCount ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.EntryCount
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ beta; alpha; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member this.``searchCollectionVocabulariesAsync sorts by entryCount descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = this.SeedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.EntryCount
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; alpha; beta ]
            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync escapes backslash in search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 528

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection @"C:\Windows" None createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "C:/Windows" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some @"C:\Windows"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary1.Id
                    Name = @"C:\Windows"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync escapes percent wildcard in search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 529

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "100% Complete" None createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "100 Complete" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some "100%"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary1.Id
                    Name = "100% Complete"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync escapes underscore wildcard in search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 530

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "word_form" None createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "wordXform" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some "word_form"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary1.Id
                    Name = "word_form"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync escapes backslash in description search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 531

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Vocab1" (Some @"Path: C:\Users") createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Vocab2" (Some "Path: C:/Users") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some @"C:\Users"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary1.Id
                    Name = "Vocab1"
                    Description = Some @"Path: C:\Users"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync escapes percent wildcard in description search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 532

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Vocab1" (Some "100% accurate") createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Vocab2" (Some "100 accurate") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some "100%"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary1.Id
                    Name = "Vocab1"
                    Description = Some "100% accurate"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync escapes underscore wildcard in description search``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 533

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Vocab1" (Some "user_name format") createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Vocab2" (Some "userXname format") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some "user_name"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary1.Id
                    Name = "Vocab1"
                    Description = Some "user_name format"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync returns all collections when search is empty string``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 534

            let collection1 =
                Entities.makeCollection user "Alpha" None createdAt None false

            let collection2 =
                Entities.makeCollection user "Beta" (Some "Description") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some ""
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Alpha"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 }
                  { Id = collection2.Id
                    UserId = user.Id
                    Name = "Beta"
                    Description = Some "Description"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchUserCollectionsAsync searches in description when name does not match``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 535

            let matchingCollection =
                Entities.makeCollection user "Alpha" (Some "Marine biology terms") createdAt None false

            let nonMatchingCollection =
                Entities.makeCollection user "Biology" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ matchingCollection; nonMatchingCollection ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.SearchUserCollectionsQuery =
                { Search = Some "marine"
                  SortBy = CollectionsHierarchy.CollectionSortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchUserCollectionsAsync user.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.CollectionOverview list =
                [ { Id = matchingCollection.Id
                    UserId = user.Id
                    Name = "Alpha"
                    Description = Some "Marine biology terms"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionOverview list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync returns all vocabularies when search is empty string``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 536

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Alpha" None createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Beta" (Some "Description") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some ""
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabulary1.Id
                    Name = "Alpha"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 }
                  { Id = vocabulary2.Id
                    Name = "Beta"
                    Description = Some "Description"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync returns empty list when search matches nothing``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 537

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Biology Terms" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some "xyz"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>([], actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync searches in description when name does not match``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 538

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let matchingVocabulary =
                Entities.makeVocabulary collection "Alpha" (Some "Marine biology terms") createdAt None false

            let nonMatchingVocabulary =
                Entities.makeVocabulary collection "Marine" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ matchingVocabulary; nonMatchingVocabulary ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = Some "biology"
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = matchingVocabulary.Id
                    Name = "Alpha"
                    Description = Some "Marine biology terms"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by updatedAt ascending with null values``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt1 =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let updatedAt2 =
                DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 539

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let vocabularyWithEarlyUpdate =
                Entities.makeVocabulary collection "Alpha" None createdAt (Some updatedAt1) false

            let vocabularyWithLateUpdate =
                Entities.makeVocabulary collection "Beta" None createdAt (Some updatedAt2) false

            let vocabularyWithNullUpdate =
                Entities.makeVocabulary collection "Gamma" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies
                    [ vocabularyWithEarlyUpdate
                      vocabularyWithLateUpdate
                      vocabularyWithNullUpdate ]
                |> Seeder.saveChangesAsync

            let query: CollectionsHierarchy.VocabularySummaryQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySummarySortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularySummary list =
                [ { Id = vocabularyWithEarlyUpdate.Id
                    Name = "Alpha"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = Some updatedAt1
                    EntryCount = 0 }
                  { Id = vocabularyWithLateUpdate.Id
                    Name = "Beta"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = Some updatedAt2
                    EntryCount = 0 }
                  { Id = vocabularyWithNullUpdate.Id
                    Name = "Gamma"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularySummary list>(expected, actual)
        }
