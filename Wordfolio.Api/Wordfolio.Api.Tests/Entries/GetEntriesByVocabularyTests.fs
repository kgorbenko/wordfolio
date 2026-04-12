namespace Wordfolio.Api.Tests.Entries

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Types
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetEntriesByVocabularyTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET by vocabulary id returns all entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(308, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry1 =
                Entities.makeEntry vocabulary "hello" now now

            let entry2 =
                Entities.makeEntry vocabulary "world" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entriesByVocabulary(collection.Id, vocabulary.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse list>()

            let actual =
                actual
                |> Seq.sortBy(_.EntryText)
                |> List.ofSeq

            let expected: EntryResponse list =
                [ { Id = actual.[0].Id
                    VocabularyId = vocabulary.Id
                    EntryText = "hello"
                    CreatedAt = actual.[0].CreatedAt
                    UpdatedAt = actual.[0].CreatedAt
                    Definitions = []
                    Translations = [] }
                  { Id = actual.[1].Id
                    VocabularyId = vocabulary.Id
                    EntryText = "world"
                    CreatedAt = actual.[1].CreatedAt
                    UpdatedAt = actual.[1].CreatedAt
                    Definitions = []
                    Translations = [] } ]

            Assert.Equal<EntryResponse list>(expected, actual)
        }

    [<Fact>]
    member _.``GET by vocabulary id returns empty list when no entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(309, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entriesByVocabulary(collection.Id, vocabulary.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<EntryResponse[]>()

            Assert.NotNull(result)
            Assert.Empty(result)
        }

    [<Fact>]
    member _.``GET by vocabulary id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url =
                Urls.Entries.entriesByVocabulary(1, 1)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by vocabulary id for non-existent vocabulary fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(310, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entriesByVocabulary(1, 999999)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by vocabulary id for another user's vocabulary fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(311, "requester@example.com", "P@ssw0rd!")

            let! _, otherWordfolioUser = factory.CreateUserAsync(312, "other@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let requesterCollection =
                Entities.makeCollection requesterWordfolioUser "Requester Collection" None now now false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None now now false

            let otherCollection =
                Entities.makeCollection otherWordfolioUser "Other Collection" None now now false

            let otherVocabulary =
                Entities.makeVocabulary otherCollection "Other Vocabulary" None now now false

            let otherEntry =
                Entities.makeEntry otherVocabulary "foreign-entry" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ requesterWordfolioUser; otherWordfolioUser ]
                |> Seeder.addCollections [ requesterCollection; otherCollection ]
                |> Seeder.addVocabularies [ requesterVocabulary; otherVocabulary ]
                |> Seeder.addEntries [ otherEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let! response = client.GetAsync(Urls.Entries.entriesByVocabulary(otherCollection.Id, otherVocabulary.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET list returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(506, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Entries.entriesByVocabulary(999999, vocabulary.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET list returns 404 when vocabulary does not belong to collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(516, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collectionA =
                Entities.makeCollection wordfolioUser "Collection A" None now now false

            let collectionB =
                Entities.makeCollection wordfolioUser "Collection B" None now now false

            let vocabularyA =
                Entities.makeVocabulary collectionA "Vocabulary A" None now now false

            let vocabularyB =
                Entities.makeVocabulary collectionB "Vocabulary B" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabularyA; vocabularyB ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Entries.entriesByVocabulary(collectionA.Id, vocabularyB.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
