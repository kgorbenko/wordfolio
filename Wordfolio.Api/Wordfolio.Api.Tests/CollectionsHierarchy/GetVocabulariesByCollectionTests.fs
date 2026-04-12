namespace Wordfolio.Api.Tests.CollectionsHierarchy

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Api.CollectionsHierarchy.Types
open Wordfolio.Api.Infrastructure.ResourceIdEncoder
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetVocabulariesByCollectionTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET vocabularies by collection endpoint returns vocabularies with entry counts``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(312, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" (Some "Description") createdAt createdAt false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt createdAt false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default" None createdAt createdAt true

            let entry =
                Entities.makeEntry regularVocabulary "word" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.CollectionsHierarchy.vocabulariesByCollection(encoder.Encode collection.Id)

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountResponse list>()

            let expected: VocabularyWithEntryCountResponse list =
                [ { Id = encoder.Encode regularVocabulary.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = createdAt
                    EntryCount = 1 } ]

            Assert.Equal<VocabularyWithEntryCountResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint returns empty list for another user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(313, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let otherUser = Entities.makeUser 314

            let requesterCollection =
                Entities.makeCollection wordfolioUser "Requester Collection" None createdAt createdAt false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None createdAt createdAt false

            let otherCollection =
                Entities.makeCollection otherUser "Other Collection" None createdAt createdAt false

            let otherVocabulary =
                Entities.makeVocabulary otherCollection "Other Vocabulary" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser; otherUser ]
                |> Seeder.addCollections [ requesterCollection; otherCollection ]
                |> Seeder.addVocabularies [ requesterVocabulary; otherVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.CollectionsHierarchy.vocabulariesByCollection(encoder.Encode otherCollection.Id)

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountResponse list>()

            Assert.Equal<VocabularyWithEntryCountResponse list>([], actual)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint returns empty list for system collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(318, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt =
                    DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

                Entities.makeCollection wordfolioUser "System Collection" None createdAt createdAt true

            let vocabulary =
                let createdAt =
                    DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

                Entities.makeVocabulary collection "Vocab" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.CollectionsHierarchy.vocabulariesByCollection(encoder.Encode collection.Id)

            let! response = client.GetAsync(url)

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyWithEntryCountResponse list>()

            Assert.Equal<VocabularyWithEntryCountResponse list>([], actual)
        }

    [<Fact>]
    member _.``GET vocabularies by collection endpoint without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let url =
                Urls.CollectionsHierarchy.vocabulariesByCollection(encoder.Encode 1)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
