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

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Original Name" (Some "Original Description") now now false

            let unaffectedVocabulary =
                Entities.makeVocabulary collection "Unaffected Vocabulary" (Some "Unaffected Description") now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary; unaffectedVocabulary ]
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

            let! actualResponse = response.Content.ReadFromJsonAsync<VocabularyResponse>()

            let expectedResponse: VocabularyResponse =
                { Id = vocabulary.Id
                  CollectionId = collection.Id
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = vocabulary.CreatedAt
                  UpdatedAt = actualResponse.UpdatedAt }

            Assert.Equal(expectedResponse, actualResponse)

            Assert.True(
                actualResponse.UpdatedAt
                >= vocabulary.CreatedAt
            )

            let! vocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            let updatedVocabulary =
                vocabularies
                |> List.find(fun currentVocabulary -> currentVocabulary.Id = vocabulary.Id)

            let unaffectedVocabularyInDatabase =
                vocabularies
                |> List.find(fun currentVocabulary -> currentVocabulary.Id = unaffectedVocabulary.Id)

            let expectedUpdatedVocabulary: Wordfolio.Vocabulary =
                { Id = vocabulary.Id
                  CollectionId = collection.Id
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = updatedVocabulary.CreatedAt
                  UpdatedAt = updatedVocabulary.UpdatedAt
                  IsDefault = false }

            let expectedUnaffectedVocabulary: Wordfolio.Vocabulary =
                { Id = unaffectedVocabulary.Id
                  CollectionId = collection.Id
                  Name = "Unaffected Vocabulary"
                  Description = Some "Unaffected Description"
                  CreatedAt = unaffectedVocabularyInDatabase.CreatedAt
                  UpdatedAt = unaffectedVocabularyInDatabase.UpdatedAt
                  IsDefault = false }

            let expectedDatabaseState =
                [ expectedUpdatedVocabulary; expectedUnaffectedVocabulary ]
                |> List.sortBy(fun currentVocabulary -> currentVocabulary.Id)

            let actualDatabaseState =
                vocabularies
                |> List.sortBy(fun currentVocabulary -> currentVocabulary.Id)

            Assert.Equal<Wordfolio.Vocabulary list>(expectedDatabaseState, actualDatabaseState)
        }

    [<Fact>]
    member _.``PUT returns 404 when vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(206, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

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

            let! databaseState = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            Assert.Equal<Wordfolio.Vocabulary list>([], databaseState)
        }

    [<Fact>]
    member _.``PUT returns 404 when vocabulary belongs to different collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(217, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let collectionA =
                Entities.makeCollection wordfolioUser "Collection A" None now now false

            let collectionB =
                Entities.makeCollection wordfolioUser "Collection B" None now now false

            let createdAt =
                DateTimeOffset(2026, 1, 10, 8, 30, 0, TimeSpan.Zero)

            let vocabulary =
                Entities.makeVocabulary
                    collectionB
                    "Original Name"
                    (Some "Original Description")
                    createdAt
                    createdAt
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let updateRequest: UpdateVocabularyRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let url =
                Urls.Vocabularies.vocabularyById(collectionA.Id, vocabulary.Id)

            let! response = client.PutAsJsonAsync(url, updateRequest)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)

            let! vocabularyInDatabaseOption =
                fixture.WordfolioSeeder
                |> Seeder.getVocabularyByIdAsync vocabulary.Id

            let expectedVocabularyInDatabase: Wordfolio.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collectionB.Id
                      Name = "Original Name"
                      Description = Some "Original Description"
                      CreatedAt = createdAt
                      UpdatedAt = createdAt
                      IsDefault = false }

            Assert.Equal(expectedVocabularyInDatabase, vocabularyInDatabaseOption)
        }

    [<Fact>]
    member _.``PUT returns 403 when updating another user's vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(213, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(214, "requester@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let ownerCollection =
                Entities.makeCollection ownerWordfolioUser "Owner Collection" None now now false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Owner Vocabulary" (Some "Owner Description") now now false

            let requesterCollection =
                Entities.makeCollection requesterWordfolioUser "Requester Collection" None now now false

            let requesterVocabulary =
                Entities.makeVocabulary
                    requesterCollection
                    "Requester Vocabulary"
                    (Some "Requester Description")
                    now
                    now
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection; requesterCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary; requesterVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let updateRequest: UpdateVocabularyRequest =
                { Name = "Unauthorized Update"
                  Description = Some "Should not be persisted" }

            let url =
                Urls.Vocabularies.vocabularyById(ownerCollection.Id, ownerVocabulary.Id)

            let! response = client.PutAsJsonAsync(url, updateRequest)

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)

            let! databaseState = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            let ownerVocabularyInDatabase =
                databaseState
                |> List.find(fun currentVocabulary -> currentVocabulary.Id = ownerVocabulary.Id)

            let requesterVocabularyInDatabase =
                databaseState
                |> List.find(fun currentVocabulary -> currentVocabulary.Id = requesterVocabulary.Id)

            let expectedOwnerVocabulary: Wordfolio.Vocabulary =
                { Id = ownerVocabulary.Id
                  CollectionId = ownerCollection.Id
                  Name = "Owner Vocabulary"
                  Description = Some "Owner Description"
                  CreatedAt = ownerVocabularyInDatabase.CreatedAt
                  UpdatedAt = ownerVocabularyInDatabase.UpdatedAt
                  IsDefault = false }

            let expectedRequesterVocabulary: Wordfolio.Vocabulary =
                { Id = requesterVocabulary.Id
                  CollectionId = requesterCollection.Id
                  Name = "Requester Vocabulary"
                  Description = Some "Requester Description"
                  CreatedAt = requesterVocabularyInDatabase.CreatedAt
                  UpdatedAt = requesterVocabularyInDatabase.UpdatedAt
                  IsDefault = false }

            let expectedDatabaseState =
                [ expectedOwnerVocabulary; expectedRequesterVocabulary ]
                |> List.sortBy(fun currentVocabulary -> currentVocabulary.Id)

            let actualDatabaseState =
                databaseState
                |> List.sortBy(fun currentVocabulary -> currentVocabulary.Id)

            Assert.Equal<Wordfolio.Vocabulary list>(expectedDatabaseState, actualDatabaseState)
        }

    [<Fact>]
    member _.``PUT with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(207, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let createdAt =
                DateTimeOffset(2026, 1, 10, 9, 0, 0, TimeSpan.Zero)

            let vocabulary =
                Entities.makeVocabulary collection "Original Name" None createdAt createdAt false

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

            let! vocabularyInDatabaseOption =
                fixture.WordfolioSeeder
                |> Seeder.getVocabularyByIdAsync vocabulary.Id

            let expectedVocabularyInDatabase: Wordfolio.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collection.Id
                      Name = "Original Name"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = createdAt
                      IsDefault = false }

            Assert.Equal(expectedVocabularyInDatabase, vocabularyInDatabaseOption)
        }

    [<Fact>]
    member _.``PUT without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(215, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let createdAt =
                DateTimeOffset(2026, 1, 10, 9, 30, 0, TimeSpan.Zero)

            let vocabulary =
                Entities.makeVocabulary
                    collection
                    "Original Name"
                    (Some "Original Description")
                    createdAt
                    createdAt
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let updateRequest: UpdateVocabularyRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let url =
                Urls.Vocabularies.vocabularyById(collection.Id, vocabulary.Id)

            let! response = client.PutAsJsonAsync(url, updateRequest)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! vocabularyInDatabaseOption =
                fixture.WordfolioSeeder
                |> Seeder.getVocabularyByIdAsync vocabulary.Id

            let expectedVocabularyInDatabase: Wordfolio.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collection.Id
                      Name = "Original Name"
                      Description = Some "Original Description"
                      CreatedAt = createdAt
                      UpdatedAt = createdAt
                      IsDefault = false }

            Assert.Equal(expectedVocabularyInDatabase, vocabularyInDatabaseOption)
        }
