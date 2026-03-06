namespace Wordfolio.Api.Tests.Collections

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Collections
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type UpdateCollectionTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``PUT updates collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(105, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection
                    wordfolioUser
                    "Original Name"
                    (Some "Original Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            let untouchedCollection =
                Entities.makeCollection
                    wordfolioUser
                    "Untouched Name"
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

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.Collections.collectionById collection.Id, updateRequest)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            let actualUpdatedCollection =
                collections
                |> List.find(fun seededCollection -> seededCollection.Id = collection.Id)

            let actualUntouchedCollection =
                collections
                |> List.find(fun seededCollection -> seededCollection.Id = untouchedCollection.Id)

            let expected: Wordfolio.Collection =
                { Id = actualUpdatedCollection.Id
                  UserId = 105
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = actualUpdatedCollection.CreatedAt
                  UpdatedAt = actualUpdatedCollection.UpdatedAt
                  IsSystem = false }

            let expectedUntouchedCollection: Wordfolio.Collection =
                { Id = untouchedCollection.Id
                  UserId = 105
                  Name = "Untouched Name"
                  Description = Some "Untouched Description"
                  CreatedAt = actualUntouchedCollection.CreatedAt
                  UpdatedAt = actualUntouchedCollection.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expected, actualUpdatedCollection)
            Assert.Equal(expectedUntouchedCollection, actualUntouchedCollection)
        }

    [<Fact>]
    member _.``PUT returns 404 when collection does not exist``() : Task =
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

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.Collections.collectionById 999999, updateRequest)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(106, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Original Name" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let updateRequest: UpdateCollectionRequest =
                { Name = ""
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.Collections.collectionById collection.Id, updateRequest)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let actual = Assert.Single(collections)

            let expected: Wordfolio.Collection =
                { Id = collection.Id
                  UserId = 106
                  Name = "Original Name"
                  Description = None
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``PUT returns 403 when updating another user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(107, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(108, "requester@example.com", "P@ssw0rd!")

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

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.Collections.collectionById ownerCollection.Id, updateRequest)

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
                  UserId = 107
                  Name = "Owner Collection"
                  Description = Some "Owner Description"
                  CreatedAt = actualOwnerCollection.CreatedAt
                  UpdatedAt = actualOwnerCollection.UpdatedAt
                  IsSystem = false }

            let expectedRequesterCollection: Wordfolio.Collection =
                { Id = requesterCollection.Id
                  UserId = 108
                  Name = "Requester Collection"
                  Description = Some "Requester Description"
                  CreatedAt = actualRequesterCollection.CreatedAt
                  UpdatedAt = actualRequesterCollection.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expectedOwnerCollection, actualOwnerCollection)
            Assert.Equal(expectedRequesterCollection, actualRequesterCollection)
        }

    [<Fact>]
    member _.``PUT without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(109, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection
                    wordfolioUser
                    "Original Name"
                    (Some "Original Description")
                    DateTimeOffset.UtcNow
                    None
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.Collections.collectionById collection.Id, updateRequest)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let actual = Assert.Single(collections)

            let expected: Wordfolio.Collection =
                { Id = collection.Id
                  UserId = 109
                  Name = "Original Name"
                  Description = Some "Original Description"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expected, actual)
        }
