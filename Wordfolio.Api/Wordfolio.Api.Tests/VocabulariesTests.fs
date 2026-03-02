namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Vocabularies.Types
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type VocabulariesTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(200, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

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
                  UpdatedAt = None }

            Assert.Equal(expectedResult, result)

            let expectedDatabaseState: Wordfolio.Vocabulary list =
                [ { Id = createdVocabularyId
                    CollectionId = collection.Id
                    Name = "Test Vocabulary"
                    Description = Some "A test vocabulary"
                    CreatedAt = result.CreatedAt
                    UpdatedAt = None
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

            use client = factory.CreateClient()

            let request: CreateVocabularyRequest =
                { Name = "Test Vocabulary"
                  Description = Some "A test vocabulary" }

            let url =
                Urls.Vocabularies.vocabulariesByCollection 1

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(201, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

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
        }

    [<Fact>]
    member _.``GET returns empty list when no vocabularies``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(202, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

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
    member _.``GET by id returns specific vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(203, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary
                    collection
                    "Test Vocabulary"
                    (Some "Test Description")
                    DateTimeOffset.UtcNow
                    None
                    false

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
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = None }

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
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

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

    [<Fact>]
    member _.``PUT updates vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(205, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary
                    collection
                    "Original Name"
                    (Some "Original Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let updateRequest: UpdateVocabularyRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let url =
                Urls.Vocabularies.vocabularyById(collection.Id, vocabulary.Id)

            let! response = client.PutAsJsonAsync(url, updateRequest)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! vocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            let actual = Assert.Single(vocabularies)

            let expected: Wordfolio.Vocabulary =
                { Id = actual.Id
                  CollectionId = collection.Id
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  IsDefault = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``PUT returns 404 when vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(206, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let updateRequest: UpdateVocabularyRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let url =
                Urls.Vocabularies.vocabularyById(collection.Id, 999999)

            let! response = client.PutAsJsonAsync(url, updateRequest)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(207, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Original Name" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let updateRequest: UpdateVocabularyRequest =
                { Name = ""
                  Description = Some "Updated Description" }

            let url =
                Urls.Vocabularies.vocabularyById(collection.Id, vocabulary.Id)

            let! response = client.PutAsJsonAsync(url, updateRequest)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let updateRequest: UpdateVocabularyRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let url =
                Urls.Vocabularies.vocabularyById(1, 1)

            let! response = client.PutAsJsonAsync(url, updateRequest)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE deletes vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(208, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabularyById(collection.Id, vocabulary.Id)

            let! response = client.DeleteAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! databaseState =
                fixture.WordfolioSeeder
                |> Seeder.getAllVocabulariesAsync

            Assert.Empty(databaseState)
        }

    [<Fact>]
    member _.``DELETE returns 404 when vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(209, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Vocabularies.vocabularyById(collection.Id, 999999)

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url =
                Urls.Vocabularies.vocabularyById(1, 1)

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
