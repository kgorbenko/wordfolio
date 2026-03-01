namespace Wordfolio.Api.DataAccess.Tests.CollectionsHierarchy

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsHierarchySearchUserCollectionsTests(fixture: WordfolioTestFixture) =
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

            let alphaSummary: CollectionsHierarchy.CollectionWithVocabularyCount =
                { Id = alpha.Id
                  UserId = user.Id
                  Name = "Alpha"
                  Description = None
                  CreatedAt = createdAt1
                  UpdatedAt = None
                  VocabularyCount = 2 }

            let betaSummary: CollectionsHierarchy.CollectionWithVocabularyCount =
                { Id = beta.Id
                  UserId = user.Id
                  Name = "Beta"
                  Description = None
                  CreatedAt = createdAt2
                  UpdatedAt = Some updatedAt2
                  VocabularyCount = 1 }

            let gammaSummary: CollectionsHierarchy.CollectionWithVocabularyCount =
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
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

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = regularCollection.Id
                    UserId = user.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 1 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 1 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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
            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Biology"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>([], actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "100% Complete"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "user_name"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = @"C:\Users"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Collection1"
                    Description = Some "100% accurate"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Collection1"
                    Description = Some "user_name format"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection1.Id
                    UserId = user.Id
                    Name = "Collection1"
                    Description = Some @"Path: C:\Users"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = collection.Id
                    UserId = user.Id
                    Name = "Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 2 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = user1Collection.Id
                    UserId = user1.Id
                    Name = "User 1 Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
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

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
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

            let expected: CollectionsHierarchy.CollectionWithVocabularyCount list =
                [ { Id = matchingCollection.Id
                    UserId = user.Id
                    Name = "Alpha"
                    Description = Some "Marine biology terms"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.CollectionWithVocabularyCount list>(expected, actual)
        }
