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

type GetVocabularyByIdTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET by id returns specific vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(203, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            let vocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Test Vocabulary" (Some "Test Description") createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabularyById(collection.Id, vocabulary.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<VocabularyDetailResponse>()

            let expected: VocabularyDetailResponse =
                { Id = vocabulary.Id
                  CollectionId = collection.Id
                  CollectionName = "Test Collection"
                  Name = "Test Vocabulary"
                  Description = Some "Test Description"
                  CreatedAt = vocabulary.CreatedAt
                  UpdatedAt = vocabulary.CreatedAt }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET by id returns 404 when vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(204, "user@example.com", "P@ssw0rd!")

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
                Urls.Vocabularies.vocabularyById(collection.Id, 999999)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 404 when vocabulary belongs to different collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(216, "user@example.com", "P@ssw0rd!")

            let collectionA =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Collection A" None createdAt createdAt false

            let collectionB =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Collection B" None createdAt createdAt false

            let vocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collectionB "Test Vocabulary" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabularyById(collectionA.Id, vocabulary.Id)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 403 when requesting another user's vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(211, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(212, "requester@example.com", "P@ssw0rd!")

            let ownerCollection =
                let createdAt = DateTimeOffset.UtcNow

                Entities.makeCollection
                    ownerWordfolioUser
                    "Owner Collection"
                    (Some "Owner Description")
                    createdAt
                    createdAt
                    false

            let ownerVocabulary =
                let createdAt = DateTimeOffset.UtcNow

                Entities.makeVocabulary
                    ownerCollection
                    "Owner Vocabulary"
                    (Some "Owner Vocabulary Description")
                    createdAt
                    createdAt
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let url =
                Urls.Vocabularies.vocabularyById(ownerCollection.Id, ownerVocabulary.Id)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url =
                Urls.Vocabularies.vocabularyById(1, 1)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
