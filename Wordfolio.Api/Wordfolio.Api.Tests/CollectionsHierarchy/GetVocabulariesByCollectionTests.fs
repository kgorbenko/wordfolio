namespace Wordfolio.Api.Tests.CollectionsHierarchy

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.CollectionsHierarchy.Types
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type private VocabulariesQueryOptions =
    { Search: string option
      SortBy: VocabularySummarySortByRequest
      SortDirection: SortDirectionRequest }

module private VocabulariesUrlHelpers =
    let private toQueryString (search: string option) (sortBy: int) (sortDirection: int) =
        let searchParam =
            search
            |> Option.map(fun search -> $"search={Uri.EscapeDataString search}")

        [ searchParam; Some $"sortBy={sortBy}"; Some $"sortDirection={sortDirection}" ]
        |> List.choose id
        |> String.concat "&"

    let vocabulariesByCollection (collectionId: int) (query: VocabulariesQueryOptions) =
        $"{Urls.CollectionsHierarchy.vocabulariesByCollection collectionId}?{toQueryString query.Search (int query.SortBy) (int query.SortDirection)}"

type GetVocabulariesByCollectionTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

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
                VocabulariesUrlHelpers.vocabulariesByCollection
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
                VocabulariesUrlHelpers.vocabulariesByCollection
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
                VocabulariesUrlHelpers.vocabulariesByCollection
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
                VocabulariesUrlHelpers.vocabulariesByCollection
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
                VocabulariesUrlHelpers.vocabulariesByCollection
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
                VocabulariesUrlHelpers.vocabulariesByCollection
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
