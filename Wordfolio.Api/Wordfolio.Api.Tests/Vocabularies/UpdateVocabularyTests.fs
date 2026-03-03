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

type UpdateVocabularyTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

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
