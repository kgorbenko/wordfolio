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

type GetCollectionsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET returns only authenticated user collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! firstIdentityUser, firstWordfolioUser = factory.CreateUserAsync(100, "user1@example.com", "P@ssw0rd!")

            let! _, secondWordfolioUser = factory.CreateUserAsync(101, "user2@example.com", "P@ssw0rd!")

            let firstUserCollection =
                let createdAt = DateTimeOffset.UtcNow

                Entities.makeCollection
                    firstWordfolioUser
                    "First User Collection"
                    (Some "Owned by first user")
                    createdAt
                    createdAt
                    false

            let secondUserCollection =
                let createdAt = DateTimeOffset.UtcNow

                Entities.makeCollection
                    secondWordfolioUser
                    "Second User Collection"
                    (Some "Owned by second user")
                    createdAt
                    createdAt
                    false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ firstWordfolioUser; secondWordfolioUser ]
                |> Seeder.addCollections [ firstUserCollection; secondUserCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(firstIdentityUser)

            let! response = client.GetAsync(Urls.Collections.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actualCollections = response.Content.ReadFromJsonAsync<CollectionResponse list>()

            let expectedCollections: CollectionResponse list =
                [ { Id = firstUserCollection.Id
                    Name = "First User Collection"
                    Description = Some "Owned by first user"
                    CreatedAt = actualCollections.Head.CreatedAt
                    UpdatedAt = actualCollections.Head.CreatedAt } ]

            Assert.Equal<CollectionResponse list>(expectedCollections, actualCollections)
        }

    [<Fact>]
    member _.``GET returns empty list when no collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Collections.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse list>()

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

            let! response = client.GetAsync(Urls.Collections.Path)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
