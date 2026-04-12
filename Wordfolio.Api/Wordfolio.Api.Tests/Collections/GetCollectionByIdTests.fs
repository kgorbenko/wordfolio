namespace Wordfolio.Api.Tests.Collections

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Collections.Types
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetCollectionByIdTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET by id returns specific collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt =
                    DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

                Entities.makeCollection
                    wordfolioUser
                    "Test Collection"
                    (Some "Test Description")
                    createdAt
                    createdAt
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Collections.collectionById collection.Id)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionResponse>()

            let expected: CollectionResponse =
                { Id = collection.Id
                  Name = "Test Collection"
                  Description = Some "Test Description"
                  CreatedAt = collection.CreatedAt
                  UpdatedAt = collection.UpdatedAt }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET by id returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(104, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Collections.collectionById 999999)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 403 when requesting another user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(105, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(106, "requester@example.com", "P@ssw0rd!")

            let ownerCollection =
                let createdAt =
                    DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

                Entities.makeCollection
                    ownerWordfolioUser
                    "Owner Collection"
                    (Some "Owner Description")
                    createdAt
                    createdAt
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let! response = client.GetAsync(Urls.Collections.collectionById ownerCollection.Id)

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.Collections.collectionById 1)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
