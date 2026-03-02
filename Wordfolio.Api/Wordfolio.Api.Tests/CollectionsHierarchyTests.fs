namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.CollectionsHierarchy
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type private CollectionsQueryOptions =
    { Search: string option
      SortBy: CollectionSortByRequest
      SortDirection: SortDirectionRequest }

type private VocabulariesQueryOptions =
    { Search: string option
      SortBy: VocabularySummarySortByRequest
      SortDirection: SortDirectionRequest }

module private UrlHelpers =
    let private toQueryString (search: string option) (sortBy: int) (sortDirection: int) =
        let searchParam =
            search
            |> Option.map(fun search -> $"search={Uri.EscapeDataString search}")

        [ searchParam; Some $"sortBy={sortBy}"; Some $"sortDirection={sortDirection}" ]
        |> List.choose id
        |> String.concat "&"

    let collections(query: CollectionsQueryOptions) =
        $"{Urls.CollectionsHierarchy.collections()}?{toQueryString query.Search (int query.SortBy) (int query.SortDirection)}"

    let vocabulariesByCollection (collectionId: int) (query: VocabulariesQueryOptions) =
        $"{Urls.CollectionsHierarchy.vocabulariesByCollection collectionId}?{toQueryString query.Search (int query.SortBy) (int query.SortDirection)}"

type CollectionsHierarchyTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET returns collections with vocabularies and entry counts``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(300, "user@example.com", "P@ssw0rd!")

            let collection1 =
                Entities.makeCollection
                    wordfolioUser
                    "Collection 1"
                    (Some "Description 1")
                    DateTimeOffset.UtcNow
                    None
                    false

            let collection2 =
                Entities.makeCollection wordfolioUser "Collection 2" None DateTimeOffset.UtcNow None false

            let vocabulary1 =
                Entities.makeVocabulary
                    collection1
                    "Vocabulary 1"
                    (Some "Vocab description")
                    DateTimeOffset.UtcNow
                    None
                    false

            let vocabulary2 =
                Entities.makeVocabulary collection1 "Vocabulary 2" None DateTimeOffset.UtcNow None false

            let entry1 =
                Entities.makeEntry vocabulary1 "entry1" DateTimeOffset.UtcNow None

            let entry2 =
                Entities.makeEntry vocabulary1 "entry2" DateTimeOffset.UtcNow None

            let entry3 =
                Entities.makeEntry vocabulary2 "entry3" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.addEntries [ entry1; entry2; entry3 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let actualCollection1 =
                actual.Collections
                |> List.find(fun c -> c.Id = collection1.Id)

            let actualCollection2 =
                actual.Collections
                |> List.find(fun c -> c.Id = collection2.Id)

            let expected: CollectionsHierarchyResponse =
                { Collections =
                    [ { Id = collection1.Id
                        Name = "Collection 1"
                        Description = Some "Description 1"
                        CreatedAt = actualCollection1.CreatedAt
                        UpdatedAt = None
                        Vocabularies =
                          [ { Id = vocabulary1.Id
                              Name = "Vocabulary 1"
                              Description = Some "Vocab description"
                              CreatedAt = actualCollection1.Vocabularies[0].CreatedAt
                              UpdatedAt = None
                              EntryCount = 2 }
                            { Id = vocabulary2.Id
                              Name = "Vocabulary 2"
                              Description = None
                              CreatedAt = actualCollection1.Vocabularies[1].CreatedAt
                              UpdatedAt = None
                              EntryCount = 1 } ] }
                      { Id = collection2.Id
                        Name = "Collection 2"
                        Description = None
                        CreatedAt = actualCollection2.CreatedAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET excludes system collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(301, "user@example.com", "P@ssw0rd!")

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None DateTimeOffset.UtcNow None false

            let systemCollection =
                Entities.makeCollection wordfolioUser "System Collection" None DateTimeOffset.UtcNow None true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ regularCollection; systemCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let actualCollection =
                Assert.Single(actual.Collections)

            let expected: CollectionsHierarchyResponse =
                { Collections =
                    [ { Id = regularCollection.Id
                        Name = "Regular Collection"
                        Description = None
                        CreatedAt = actualCollection.CreatedAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET excludes default vocabularies``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(302, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular Vocabulary" None DateTimeOffset.UtcNow None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default Vocabulary" None DateTimeOffset.UtcNow None true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let actualCollection =
                Assert.Single(actual.Collections)

            let expected: CollectionsHierarchyResponse =
                { Collections =
                    [ { Id = collection.Id
                        Name = "Test Collection"
                        Description = None
                        CreatedAt = actualCollection.CreatedAt
                        UpdatedAt = None
                        Vocabularies =
                          [ { Id = regularVocabulary.Id
                              Name = "Regular Vocabulary"
                              Description = None
                              CreatedAt = actualCollection.Vocabularies[0].CreatedAt
                              UpdatedAt = None
                              EntryCount = 0 } ] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET returns empty list when no collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(303, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let expected: CollectionsHierarchyResponse =
                { Collections = []
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET returns default vocabulary when it has entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(304, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                Entities.makeCollection wordfolioUser "Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary
                    systemCollection
                    "My Words"
                    (Some "Default vocab")
                    DateTimeOffset.UtcNow
                    None
                    true

            let entry1 =
                Entities.makeEntry defaultVocabulary "word1" DateTimeOffset.UtcNow None

            let entry2 =
                Entities.makeEntry defaultVocabulary "word2" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let expected: CollectionsHierarchyResponse =
                { Collections = []
                  DefaultVocabulary =
                    Some
                        { Id = defaultVocabulary.Id
                          Name = "My Words"
                          Description = Some "Default vocab"
                          CreatedAt = actual.DefaultVocabulary.Value.CreatedAt
                          UpdatedAt = None
                          EntryCount = 2 } }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET does not return default vocabulary when it has no entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(305, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                Entities.makeCollection wordfolioUser "Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "My Words" None DateTimeOffset.UtcNow None true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let expected: CollectionsHierarchyResponse =
                { Collections = []
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET collections list endpoint supports filtering and sorting``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(306, "user@example.com", "P@ssw0rd!")

            let createdAt1 =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let createdAt2 =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let updatedAt1 =
                DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero)

            let updatedAt2 =
                DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero)

            let collection1 =
                Entities.makeCollection wordfolioUser "Biology" (Some "School words") createdAt1 (Some updatedAt1) false

            let collection2 =
                Entities.makeCollection wordfolioUser "Travel" (Some "Bio terms") createdAt2 (Some updatedAt2) false

            let collection3 =
                Entities.makeCollection wordfolioUser "Sports" None createdAt2 None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection1; collection2; collection3 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.collections
                    { Search = Some "bio"
                      SortBy = CollectionSortByRequest.UpdatedAt
                      SortDirection = SortDirectionRequest.Desc }

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionOverviewResponse list>()

            let expected: CollectionOverviewResponse list =
                [ { Id = collection2.Id
                    Name = "Travel"
                    Description = Some "Bio terms"
                    CreatedAt = createdAt2
                    UpdatedAt = Some updatedAt2
                    VocabularyCount = 0 }
                  { Id = collection1.Id
                    Name = "Biology"
                    Description = Some "School words"
                    CreatedAt = createdAt1
                    UpdatedAt = Some updatedAt1
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionOverviewResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET collections list endpoint returns only collections owned by user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(310, "user@example.com", "P@ssw0rd!")

            let otherUser = Entities.makeUser 311

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let ownedCollection =
                Entities.makeCollection wordfolioUser "Owned" None createdAt None false

            let otherCollection =
                Entities.makeCollection otherUser "Other" None createdAt None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser; otherUser ]
                |> Seeder.addCollections [ ownedCollection; otherCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.collections
                    { Search = None
                      SortBy = CollectionSortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionOverviewResponse list>()

            let expected: CollectionOverviewResponse list =
                [ { Id = ownedCollection.Id
                    Name = "Owned"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionOverviewResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET collections list endpoint returns vocabulary count excluding system and default vocabularies``
        ()
        : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(317, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Regular" None createdAt None false

            let systemCollection =
                Entities.makeCollection wordfolioUser "System" None createdAt None true

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular Vocab" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default Vocab" None createdAt None true

            let systemCollectionVocabulary =
                Entities.makeVocabulary systemCollection "System Vocab" None createdAt None false

            let entry1 =
                Entities.makeEntry regularVocabulary "word1" createdAt None

            let entry2 =
                Entities.makeEntry regularVocabulary "word2" createdAt None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection; systemCollection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary; systemCollectionVocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.collections
                    { Search = None
                      SortBy = CollectionSortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionOverviewResponse list>()

            let expected: CollectionOverviewResponse list =
                [ { Id = collection.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 1 } ]

            Assert.Equal<CollectionOverviewResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET collections list endpoint without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url =
                UrlHelpers.collections
                    { Search = None
                      SortBy = CollectionSortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint returns vocabularies with entry counts``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(312, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" (Some "Description") createdAt None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default" None createdAt None true

            let entry =
                Entities.makeEntry regularVocabulary "word" createdAt None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.vocabulariesByCollection
                    collection.Id
                    { Search = None
                      SortBy = VocabularySummarySortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountHierarchyResponse list>()

            let expected: VocabularyWithEntryCountHierarchyResponse list =
                [ { Id = regularVocabulary.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 1 } ]

            Assert.Equal<VocabularyWithEntryCountHierarchyResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint returns empty list for non-owned collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(313, "user@example.com", "P@ssw0rd!")

            let otherUser = Entities.makeUser 314

            let collection =
                Entities.makeCollection otherUser "Other Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocab" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser; otherUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.vocabulariesByCollection
                    collection.Id
                    { Search = None
                      SortBy = VocabularySummarySortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountHierarchyResponse list>()

            Assert.Equal<VocabularyWithEntryCountHierarchyResponse list>([], actual)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint returns empty list for system collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(318, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "System Collection" None DateTimeOffset.UtcNow None true

            let vocabulary =
                Entities.makeVocabulary collection "Vocab" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.vocabulariesByCollection
                    collection.Id
                    { Search = None
                      SortBy = VocabularySummarySortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountHierarchyResponse list>()

            Assert.Equal<VocabularyWithEntryCountHierarchyResponse list>([], actual)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url =
                UrlHelpers.vocabulariesByCollection
                    1
                    { Search = None
                      SortBy = VocabularySummarySortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint supports search filtering``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(315, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Biology Terms" None createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Travel Words" None createdAt None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.vocabulariesByCollection
                    collection.Id
                    { Search = Some "bio"
                      SortBy = VocabularySummarySortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountHierarchyResponse list>()

            let expected: VocabularyWithEntryCountHierarchyResponse list =
                [ { Id = vocabulary1.Id
                    Name = "Biology Terms"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<VocabularyWithEntryCountHierarchyResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint supports sorting``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(316, "user@example.com", "P@ssw0rd!")

            let createdAt1 =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let createdAt2 =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None createdAt1 None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Zebra" None createdAt1 None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Apple" None createdAt2 None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                UrlHelpers.vocabulariesByCollection
                    collection.Id
                    { Search = None
                      SortBy = VocabularySummarySortByRequest.Name
                      SortDirection = SortDirectionRequest.Asc }

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountHierarchyResponse list>()

            let expected: VocabularyWithEntryCountHierarchyResponse list =
                [ { Id = vocabulary2.Id
                    Name = "Apple"
                    Description = None
                    CreatedAt = createdAt2
                    UpdatedAt = None
                    EntryCount = 0 }
                  { Id = vocabulary1.Id
                    Name = "Zebra"
                    Description = None
                    CreatedAt = createdAt1
                    UpdatedAt = None
                    EntryCount = 0 } ]

            Assert.Equal<VocabularyWithEntryCountHierarchyResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET /collections-hierarchy without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
