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

            let timestamp =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None timestamp timestamp false

            let untouchedCollection =
                Entities.makeCollection
                    wordfolioUser
                    "Untouched Collection"
                    (Some "Untouched Description")
                    timestamp
                    timestamp
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
                  CreatedAt = untouchedCollection.CreatedAt
                  UpdatedAt = untouchedCollection.UpdatedAt
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

            let timestamp =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection with content" None timestamp timestamp false

            let untouchedCollection =
                Entities.makeCollection
                    wordfolioUser
                    "Untouched Collection"
                    (Some "Untouched Description")
                    timestamp
                    timestamp
                    false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None timestamp timestamp false

            let untouchedVocabulary =
                Entities.makeVocabulary untouchedCollection "Untouched Vocabulary" None timestamp timestamp false

            let entry =
                Entities.makeEntry vocabulary "word" timestamp timestamp

            let untouchedEntry =
                Entities.makeEntry untouchedVocabulary "untouched word" timestamp timestamp

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
                  CreatedAt = untouchedCollection.CreatedAt
                  UpdatedAt = untouchedCollection.UpdatedAt
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
                  CreatedAt = untouchedVocabulary.CreatedAt
                  UpdatedAt = untouchedVocabulary.UpdatedAt
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
                  CreatedAt = untouchedEntry.CreatedAt
                  UpdatedAt = untouchedEntry.UpdatedAt }

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
                let createdAt =
                    DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

                Entities.makeCollection
                    wordfolioUser
                    "Protected Collection"
                    (Some "Protected Description")
                    createdAt
                    createdAt
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
                  CreatedAt = collection.CreatedAt
                  UpdatedAt = collection.UpdatedAt
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

            let timestamp =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let ownerCollection =
                Entities.makeCollection
                    ownerWordfolioUser
                    "Owner Collection"
                    (Some "Owner Description")
                    timestamp
                    timestamp
                    false

            let requesterCollection =
                Entities.makeCollection
                    requesterWordfolioUser
                    "Requester Collection"
                    (Some "Requester Description")
                    timestamp
                    timestamp
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
                  CreatedAt = ownerCollection.CreatedAt
                  UpdatedAt = ownerCollection.UpdatedAt
                  IsSystem = false }

            let expectedRequesterCollection: Wordfolio.Collection =
                { Id = requesterCollection.Id
                  UserId = 111
                  Name = "Requester Collection"
                  Description = Some "Requester Description"
                  CreatedAt = requesterCollection.CreatedAt
                  UpdatedAt = requesterCollection.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expectedOwnerCollection, actualOwnerCollection)
            Assert.Equal(expectedRequesterCollection, actualRequesterCollection)
        }
