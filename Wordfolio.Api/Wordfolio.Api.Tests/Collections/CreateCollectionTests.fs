namespace Wordfolio.Api.Tests.Collections

open System.Net
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Api.Collections.Types
open Wordfolio.Api.Infrastructure.ResourceIdEncoder
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type CreateCollectionTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            let createdCollectionId = result.Id

            let createdCollectionDatabaseId =
                createdCollectionId
                |> encoder.Decode
                |> Option.get

            let expectedResult: CollectionResponse =
                { Id = createdCollectionId
                  Name = "My Collection"
                  Description = Some "A test collection"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = result.CreatedAt }

            Assert.Equal(expectedResult, result)

            let! databaseState = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            let expectedDatabaseState: Wordfolio.Collection list =
                [ { Id = createdCollectionDatabaseId
                    UserId = 100
                    Name = "My Collection"
                    Description = Some "A test collection"
                    CreatedAt = result.CreatedAt
                    UpdatedAt = result.CreatedAt
                    IsSystem = false } ]

            Assert.Equal<Collection list>(expectedDatabaseState, databaseState)
        }

    [<Fact>]
    member _.``POST without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections.Path, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! databaseState = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            Assert.Equal<Collection list>([], databaseState)
        }

    [<Fact>]
    member _.``POST with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateCollectionRequest =
                { Name = ""
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }
