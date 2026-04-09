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

type CreateVocabularyTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(200, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateVocabularyRequest =
                { Name = "Test Vocabulary"
                  Description = Some "A test vocabulary" }

            let url =
                Urls.Vocabularies.vocabulariesByCollection collection.Id

            let! response = client.PostAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<VocabularyResponse>()

            let createdVocabularyId = result.Id

            let expectedResult: VocabularyResponse =
                { Id = createdVocabularyId
                  CollectionId = collection.Id
                  Name = "Test Vocabulary"
                  Description = Some "A test vocabulary"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = result.CreatedAt }

            Assert.Equal(expectedResult, result)

            let expectedDatabaseState: Wordfolio.Vocabulary list =
                [ { Id = createdVocabularyId
                    CollectionId = collection.Id
                    Name = "Test Vocabulary"
                    Description = Some "A test vocabulary"
                    CreatedAt = result.CreatedAt
                    UpdatedAt = result.CreatedAt
                    IsDefault = false } ]

            let! databaseState = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            Assert.Equal<Vocabulary list>(expectedDatabaseState, databaseState)
        }

    [<Fact>]
    member _.``POST without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(202, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let request: CreateVocabularyRequest =
                { Name = "Test Vocabulary"
                  Description = Some "A test vocabulary" }

            let url =
                Urls.Vocabularies.vocabulariesByCollection collection.Id

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! databaseState = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            Assert.Equal<Vocabulary list>([], databaseState)
        }

    [<Fact>]
    member _.``POST create vocabulary for another user's collection fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(206, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(207, "requester@example.com", "P@ssw0rd!")

            let ownerCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection ownerWordfolioUser "Owner Collection" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let request: CreateVocabularyRequest =
                { Name = "Unauthorized Vocabulary"
                  Description = Some "Should not be created" }

            let url =
                Urls.Vocabularies.vocabulariesByCollection ownerCollection.Id

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)

            let! databaseState = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            Assert.Equal<Vocabulary list>([], databaseState)
        }

    [<Fact>]
    member _.``POST with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(201, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateVocabularyRequest =
                { Name = ""
                  Description = Some "A test vocabulary" }

            let url =
                Urls.Vocabularies.vocabulariesByCollection collection.Id

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! databaseState = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            Assert.Equal<Vocabulary list>([], databaseState)
        }
