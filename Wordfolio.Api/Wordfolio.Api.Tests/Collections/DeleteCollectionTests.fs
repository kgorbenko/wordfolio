namespace Wordfolio.Api.Tests.Collections

open System
open System.Net
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type DeleteCollectionTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``DELETE deletes collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(106, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let untouchedCollection =
                Entities.makeCollection
                    wordfolioUser
                    "Untouched Collection"
                    (Some "Untouched Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection; untouchedCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.DeleteAsync(Urls.Collections.collectionById collection.Id)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! databaseState =
                fixture.WordfolioSeeder
                |> Seeder.getAllCollectionsAsync

            let actual = Assert.Single(databaseState)

            let expected: Wordfolio.Collection =
                { Id = untouchedCollection.Id
                  UserId = 106
                  Name = "Untouched Collection"
                  Description = Some "Untouched Description"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``DELETE deletes collection with vocabularies and entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(112, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Collection with content" None DateTimeOffset.UtcNow None false

            let untouchedCollection =
                Entities.makeCollection
                    wordfolioUser
                    "Untouched Collection"
                    (Some "Untouched Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None DateTimeOffset.UtcNow None false

            let untouchedVocabulary =
                Entities.makeVocabulary untouchedCollection "Untouched Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabulary "word" DateTimeOffset.UtcNow None

            let untouchedEntry =
                Entities.makeEntry untouchedVocabulary "untouched word" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection; untouchedCollection ]
                |> Seeder.addVocabularies [ vocabulary; untouchedVocabulary ]
                |> Seeder.addEntries [ entry; untouchedEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.DeleteAsync(Urls.Collections.collectionById collection.Id)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! actualCollections =
                fixture.WordfolioSeeder
                |> Seeder.getAllCollectionsAsync

            let actualCollection =
                Assert.Single(actualCollections)

            let expectedCollection: Wordfolio.Collection =
                { Id = untouchedCollection.Id
                  UserId = 112
                  Name = "Untouched Collection"
                  Description = Some "Untouched Description"
                  CreatedAt = actualCollection.CreatedAt
                  UpdatedAt = actualCollection.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expectedCollection, actualCollection)

            let! actualVocabularies =
                fixture.WordfolioSeeder
                |> Seeder.getAllVocabulariesAsync

            let actualVocabulary =
                Assert.Single(actualVocabularies)

            let expectedVocabulary: Wordfolio.Vocabulary =
                { Id = untouchedVocabulary.Id
                  CollectionId = untouchedCollection.Id
                  Name = "Untouched Vocabulary"
                  Description = None
                  CreatedAt = actualVocabulary.CreatedAt
                  UpdatedAt = actualVocabulary.UpdatedAt
                  IsDefault = false }

            Assert.Equal(expectedVocabulary, actualVocabulary)

            let! actualEntries =
                fixture.WordfolioSeeder
                |> Seeder.getAllEntriesAsync

            let actualEntry =
                Assert.Single(actualEntries)

            let expectedEntry: Wordfolio.Entry =
                { Id = untouchedEntry.Id
                  VocabularyId = untouchedVocabulary.Id
                  EntryText = "untouched word"
                  CreatedAt = actualEntry.CreatedAt
                  UpdatedAt = actualEntry.UpdatedAt }

            Assert.Equal(expectedEntry, actualEntry)
        }

    [<Fact>]
    member _.``DELETE returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(106, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.DeleteAsync(Urls.Collections.collectionById 999999)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(109, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection
                    wordfolioUser
                    "Protected Collection"
                    (Some "Protected Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let! response = client.DeleteAsync(Urls.Collections.collectionById collection.Id)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let actual = Assert.Single(collections)

            let expected: Wordfolio.Collection =
                { Id = collection.Id
                  UserId = 109
                  Name = "Protected Collection"
                  Description = Some "Protected Description"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``DELETE returns 403 when deleting another user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(110, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(111, "requester@example.com", "P@ssw0rd!")

            let ownerCollection =
                Entities.makeCollection
                    ownerWordfolioUser
                    "Owner Collection"
                    (Some "Owner Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            let requesterCollection =
                Entities.makeCollection
                    requesterWordfolioUser
                    "Requester Collection"
                    (Some "Requester Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection; requesterCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let! response = client.DeleteAsync(Urls.Collections.collectionById ownerCollection.Id)

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            let actualOwnerCollection =
                collections
                |> List.find(fun seededCollection -> seededCollection.Id = ownerCollection.Id)

            let actualRequesterCollection =
                collections
                |> List.find(fun seededCollection -> seededCollection.Id = requesterCollection.Id)

            let expectedOwnerCollection: Wordfolio.Collection =
                { Id = ownerCollection.Id
                  UserId = 110
                  Name = "Owner Collection"
                  Description = Some "Owner Description"
                  CreatedAt = actualOwnerCollection.CreatedAt
                  UpdatedAt = actualOwnerCollection.UpdatedAt
                  IsSystem = false }

            let expectedRequesterCollection: Wordfolio.Collection =
                { Id = requesterCollection.Id
                  UserId = 111
                  Name = "Requester Collection"
                  Description = Some "Requester Description"
                  CreatedAt = actualRequesterCollection.CreatedAt
                  UpdatedAt = actualRequesterCollection.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expectedOwnerCollection, actualOwnerCollection)
            Assert.Equal(expectedRequesterCollection, actualRequesterCollection)
        }
