namespace Wordfolio.Api.Tests.Vocabularies

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Api.Vocabularies.Types
open Wordfolio.Api.Infrastructure.ResourceIdEncoder
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

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(208, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "My Collection" None now now false

            let firstVocabulary =
                Entities.makeVocabulary collection "Animals" (Some "Animal words") now now false

            let secondVocabulary =
                Entities.makeVocabulary collection "Travel" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ firstVocabulary; secondVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabulariesByCollection(encoder.Encode collection.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<VocabularyResponse[]>()

            Assert.NotNull(result)

            let sortedResult =
                result |> Array.sortBy _.Id

            let expected: VocabularyResponse[] =
                [| { Id = encoder.Encode firstVocabulary.Id
                     CollectionId = encoder.Encode collection.Id
                     Name = "Animals"
                     Description = Some "Animal words"
                     CreatedAt = sortedResult[0].CreatedAt
                     UpdatedAt = sortedResult[0].CreatedAt }
                   { Id = encoder.Encode secondVocabulary.Id
                     CollectionId = encoder.Encode collection.Id
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

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(202, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabulariesByCollection(encoder.Encode collection.Id)

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

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let url =
                Urls.Vocabularies.vocabulariesByCollection(encoder.Encode 1)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET vocabularies for another user's collection returns not found``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! _, ownerWordfolioUser = factory.CreateUserAsync(209, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(210, "requester@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let ownerCollection =
                Entities.makeCollection ownerWordfolioUser "Owner Collection" None now now false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Owner Vocabulary" None now now false

            let requesterCollection =
                Entities.makeCollection requesterWordfolioUser "Requester Collection" None now now false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection; requesterCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary; requesterVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let url =
                Urls.Vocabularies.vocabulariesByCollection(encoder.Encode ownerCollection.Id)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
