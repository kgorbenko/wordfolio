namespace Wordfolio.Api.DataAccess.Tests.CollectionsHierarchy

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsHierarchySearchCollectionVocabulariesTests(fixture: WordfolioTestFixture) =
    let seedVocabulariesForSortingTests() =
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

            let alphaSummary: CollectionsHierarchy.VocabularyWithEntryCount =
                { Id = alpha.Id
                  Name = "Alpha"
                  Description = None
                  CreatedAt = createdAt1
                  UpdatedAt = Some updatedAt1
                  EntryCount = 2 }

            let betaSummary: CollectionsHierarchy.VocabularyWithEntryCount =
                { Id = beta.Id
                  Name = "Beta"
                  Description = None
                  CreatedAt = createdAt2
                  UpdatedAt = Some updatedAt2
                  EntryCount = 1 }

            let gammaSummary: CollectionsHierarchy.VocabularyWithEntryCount =
                { Id = gamma.Id
                  Name = "Gamma"
                  Description = None
                  CreatedAt = createdAt3
                  UpdatedAt = Some updatedAt3
                  EntryCount = 3 }

            return user.Id, collection.Id, alphaSummary, betaSummary, gammaSummary
        }

    interface IClassFixture<WordfolioTestFixture>

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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = regularVocabulary.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 2 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id 99999 query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user1.Id user2Collection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id systemCollection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some "BIO"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary.Id
                    Name = "Biology Terms"
                    Description = Some "Science vocab"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by name ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by name descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by createdAt ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.CreatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by createdAt descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.CreatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by updatedAt ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; beta; alpha ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by updatedAt descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ alpha; beta; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by entryCount ascending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.EntryCount
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ beta; alpha; gamma ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }

    [<Fact>]
    member _.``searchCollectionVocabulariesAsync sorts by entryCount descending``() =
        task {
            let! userId, collectionId, alpha, beta, gamma = seedVocabulariesForSortingTests()

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.EntryCount
                  SortDirection = CollectionsHierarchy.SortDirection.Desc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync userId collectionId query
                |> fixture.WithConnectionAsync

            let expected = [ gamma; alpha; beta ]
            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some @"C:\Windows"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary1.Id
                    Name = @"C:\Windows"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some "100%"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary1.Id
                    Name = "100% Complete"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some "word_form"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary1.Id
                    Name = "word_form"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some @"C:\Users"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary1.Id
                    Name = "Vocab1"
                    Description = Some @"Path: C:\Users"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some "100%"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary1.Id
                    Name = "Vocab1"
                    Description = Some "100% accurate"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some "user_name"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = vocabulary1.Id
                    Name = "Vocab1"
                    Description = Some "user_name format"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some ""
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
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

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some "xyz"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>([], actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = Some "biology"
                  SortBy = CollectionsHierarchy.VocabularySortBy.Name
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
                [ { Id = matchingVocabulary.Id
                    Name = "Alpha"
                    Description = Some "Marine biology terms"
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
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

            let query: CollectionsHierarchy.SearchCollectionVocabulariesQuery =
                { Search = None
                  SortBy = CollectionsHierarchy.VocabularySortBy.UpdatedAt
                  SortDirection = CollectionsHierarchy.SortDirection.Asc }

            let! actual =
                CollectionsHierarchy.searchCollectionVocabulariesAsync user.Id collection.Id query
                |> fixture.WithConnectionAsync

            let expected: CollectionsHierarchy.VocabularyWithEntryCount list =
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

            Assert.Equal<CollectionsHierarchy.VocabularyWithEntryCount list>(expected, actual)
        }
