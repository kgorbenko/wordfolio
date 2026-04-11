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

type MoveVocabularyTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST move updates vocabulary collection and returns updated vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(600, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let createdAt =
                DateTimeOffset(2026, 1, 10, 10, 0, 0, TimeSpan.Zero)

            let sourceCollection =
                Entities.makeCollection wordfolioUser "Source Collection" None now now false

            let targetCollection =
                Entities.makeCollection wordfolioUser "Target Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary sourceCollection "My Vocabulary" None createdAt createdAt false

            let unaffectedVocabulary =
                Entities.makeVocabulary sourceCollection "Stay Here" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ sourceCollection; targetCollection ]
                |> Seeder.addVocabularies [ vocabulary; unaffectedVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveVocabularyRequest =
                { CollectionId = targetCollection.Id }

            let url =
                Urls.Vocabularies.moveVocabularyById(sourceCollection.Id, vocabulary.Id)

            let! response = client.PostAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<VocabularyResponse>()

            let expected: VocabularyResponse =
                { Id = vocabulary.Id
                  CollectionId = targetCollection.Id
                  Name = "My Vocabulary"
                  Description = None
                  CreatedAt = createdAt
                  UpdatedAt = actual.UpdatedAt }

            Assert.Equal(expected, actual)
            Assert.True(actual.UpdatedAt >= createdAt)

            let! movedVocabulary =
                fixture.WordfolioSeeder
                |> Seeder.getVocabularyByIdAsync vocabulary.Id

            let expectedMovedVocabulary: Wordfolio.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = targetCollection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = actual.UpdatedAt
                      IsDefault = false }

            Assert.Equal(expectedMovedVocabulary, movedVocabulary)

            let! dbVocabularies =
                fixture.WordfolioSeeder
                |> Seeder.getAllVocabulariesAsync

            let unaffectedInDb =
                dbVocabularies
                |> List.find(fun v -> v.Id = unaffectedVocabulary.Id)

            let expectedUnaffected: Wordfolio.Vocabulary =
                { unaffectedInDb with
                    Id = unaffectedVocabulary.Id
                    CollectionId = sourceCollection.Id
                    Name = "Stay Here" }

            Assert.Equal(expectedUnaffected, unaffectedInDb)
        }

    [<Fact>]
    member _.``POST move without authentication returns 401``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: MoveVocabularyRequest =
                { CollectionId = 999 }

            let! response = client.PostAsJsonAsync(Urls.Vocabularies.moveVocabularyById(1, 1), request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(601, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveVocabularyRequest =
                { CollectionId = collection.Id }

            let! response = client.PostAsJsonAsync(Urls.Vocabularies.moveVocabularyById(collection.Id, 999999), request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when target collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(602, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "My Vocabulary" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveVocabularyRequest =
                { CollectionId = 999999 }

            let! response =
                client.PostAsJsonAsync(Urls.Vocabularies.moveVocabularyById(collection.Id, vocabulary.Id), request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when vocabulary belongs to different source collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(603, "user@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let collectionA =
                Entities.makeCollection wordfolioUser "Collection A" None now now false

            let collectionB =
                Entities.makeCollection wordfolioUser "Collection B" None now now false

            let vocabulary =
                Entities.makeVocabulary collectionB "My Vocabulary" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveVocabularyRequest =
                { CollectionId = collectionA.Id }

            let! response =
                client.PostAsJsonAsync(Urls.Vocabularies.moveVocabularyById(collectionA.Id, vocabulary.Id), request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)

            let! vocabularyInDatabase =
                fixture.WordfolioSeeder
                |> Seeder.getVocabularyByIdAsync vocabulary.Id

            let expectedVocabularyInDatabase: Wordfolio.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collectionB.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = vocabularyInDatabase.Value.CreatedAt
                      UpdatedAt = vocabularyInDatabase.Value.CreatedAt
                      IsDefault = false }

            Assert.Equal(expectedVocabularyInDatabase, vocabularyInDatabase)
        }

    [<Fact>]
    member _.``POST move returns 404 when target collection belongs to another user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(604, "user1@example.com", "P@ssw0rd!")
            let! _, wordfolioUser2 = factory.CreateUserAsync(605, "user2@example.com", "P@ssw0rd!")

            let now = DateTimeOffset.UtcNow

            let sourceCollection =
                Entities.makeCollection wordfolioUser1 "Source" None now now false

            let vocabulary =
                Entities.makeVocabulary sourceCollection "My Vocabulary" None now now false

            let foreignCollection =
                Entities.makeCollection wordfolioUser2 "Foreign" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ sourceCollection; foreignCollection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser1)

            let request: MoveVocabularyRequest =
                { CollectionId = foreignCollection.Id }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Vocabularies.moveVocabularyById(sourceCollection.Id, vocabulary.Id),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)

            let! vocabularyInDatabase =
                fixture.WordfolioSeeder
                |> Seeder.getVocabularyByIdAsync vocabulary.Id

            let expectedVocabularyInDatabase: Wordfolio.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = sourceCollection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = vocabularyInDatabase.Value.CreatedAt
                      UpdatedAt = vocabularyInDatabase.Value.CreatedAt
                      IsDefault = false }

            Assert.Equal(expectedVocabularyInDatabase, vocabularyInDatabase)
        }
