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

module Urls = Wordfolio.Api.Urls.CollectionsHierarchy

type GetCollectionsListTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET collections list endpoint returns all user collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(306, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt1 =
                DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero)

            let updatedAt2 =
                DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero)

            let collection1 =
                Entities.makeCollection wordfolioUser "Biology" (Some "School words") createdAt (Some updatedAt1) false

            let collection2 =
                Entities.makeCollection wordfolioUser "Travel" (Some "Bio terms") createdAt (Some updatedAt2) false

            let collection3 =
                Entities.makeCollection wordfolioUser "Sports" None createdAt None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection1; collection2; collection3 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.collections())

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionWithVocabularyCountResponse list>()

            let expected: CollectionWithVocabularyCountResponse list =
                [ { Id = collection2.Id
                    Name = "Travel"
                    Description = Some "Bio terms"
                    CreatedAt = createdAt
                    UpdatedAt = Some updatedAt2
                    VocabularyCount = 0 }
                  { Id = collection1.Id
                    Name = "Biology"
                    Description = Some "School words"
                    CreatedAt = createdAt
                    UpdatedAt = Some updatedAt1
                    VocabularyCount = 0 }
                  { Id = collection3.Id
                    Name = "Sports"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionWithVocabularyCountResponse list>(expected, actual)
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

            let! response = client.GetAsync(Urls.collections())

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionWithVocabularyCountResponse list>()

            let expected: CollectionWithVocabularyCountResponse list =
                [ { Id = ownedCollection.Id
                    Name = "Owned"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 0 } ]

            Assert.Equal<CollectionWithVocabularyCountResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET collections list endpoint returns empty list when user has no collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(319, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.collections())

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionWithVocabularyCountResponse list>()

            Assert.Equal<CollectionWithVocabularyCountResponse list>([], actual)
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

            let! response = client.GetAsync(Urls.collections())
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionWithVocabularyCountResponse list>()

            let expected: CollectionWithVocabularyCountResponse list =
                [ { Id = collection.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None
                    VocabularyCount = 1 } ]

            Assert.Equal<CollectionWithVocabularyCountResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET collections list endpoint without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.collections())

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
