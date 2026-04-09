namespace Wordfolio.Api.Tests.Vocabularies

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Vocabularies.Types
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetVocabulariesTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET returns vocabularies for authenticated user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(208, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "My Collection" None createdAt createdAt false

            let firstVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Animals" (Some "Animal words") createdAt createdAt false

            let secondVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Travel" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ firstVocabulary; secondVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabulariesByCollection collection.Id

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<VocabularyResponse[]>()

            Assert.NotNull(result)

            let sortedResult =
                result |> Array.sortBy _.Id

            let expected: VocabularyResponse[] =
                [| { Id = firstVocabulary.Id
                     CollectionId = collection.Id
                     Name = "Animals"
                     Description = Some "Animal words"
                     CreatedAt = sortedResult[0].CreatedAt
                     UpdatedAt = sortedResult[0].CreatedAt }
                   { Id = secondVocabulary.Id
                     CollectionId = collection.Id
                     Name = "Travel"
                     Description = None
                     CreatedAt = sortedResult[1].CreatedAt
                     UpdatedAt = sortedResult[1].CreatedAt } |]
                |> Array.sortBy _.Id

            Assert.Equal<VocabularyResponse>(expected, sortedResult)
        }

    [<Fact>]
    member _.``GET returns empty list when no vocabularies``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(202, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabulariesByCollection collection.Id

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<VocabularyResponse[]>()

            Assert.NotNull(result)
            Assert.Empty(result)
        }

    [<Fact>]
    member _.``GET without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url =
                Urls.Vocabularies.vocabulariesByCollection 1

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET vocabularies for another user's collection returns not found``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(209, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(210, "requester@example.com", "P@ssw0rd!")

            let ownerCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection ownerWordfolioUser "Owner Collection" None createdAt createdAt false

            let ownerVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary ownerCollection "Owner Vocabulary" None createdAt createdAt false

            let requesterCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection requesterWordfolioUser "Requester Collection" None createdAt createdAt false

            let requesterVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection; requesterCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary; requesterVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let url =
                Urls.Vocabularies.vocabulariesByCollection ownerCollection.Id

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
