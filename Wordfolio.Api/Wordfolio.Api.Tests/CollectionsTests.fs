namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Handlers.Collections
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type CollectionsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates collection``() : Task =
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

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            let createdCollectionId = result.Id

            let expectedResult: CollectionResponse =
                { Id = createdCollectionId
                  Name = "My Collection"
                  Description = Some "A test collection"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = None }

            Assert.Equal(expectedResult, result)

            let expectedDatabaseState: Wordfolio.Collection list =
                [ { Id = createdCollectionId
                    UserId = 100
                    Name = "My Collection"
                    Description = Some "A test collection"
                    CreatedAt = result.CreatedAt
                    UpdatedAt = None
                    IsSystem = false } ]

            let! databaseState = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            Assert.Equal<Collection list>(expectedDatabaseState, databaseState)
        }

    [<Fact>]
    member _.``POST without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections.Path, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST with empty name fails``() : Task =
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

            let request: CreateCollectionRequest =
                { Name = ""
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
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

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse[]>()

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

    [<Fact>]
    member _.``GET by id returns specific collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection
                    wordfolioUser
                    "Test Collection"
                    (Some "Test Description")
                    DateTimeOffset.UtcNow
                    None
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

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            Assert.NotNull(result)

            let expected: CollectionResponse =
                { Id = collection.Id
                  Name = "Test Collection"
                  Description = Some "Test Description"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = None }

            Assert.Equal(expected, result)
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
    member _.``GET by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.Collections.collectionById 1)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

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

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.Collections.collectionById collection.Id, updateRequest)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            Assert.NotNull(result)
            Assert.True(result.UpdatedAt.IsSome)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let collection = Assert.Single(collections)

            let expected: Wordfolio.Collection =
                { Id = collection.Id
                  UserId = 105
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = collection.CreatedAt
                  UpdatedAt = collection.UpdatedAt
                  IsSystem = false }

            Assert.Equal(expected, collection)
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
        }

    [<Fact>]
    member _.``PUT without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.Collections.collectionById 1, updateRequest)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE deletes collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(106, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.DeleteAsync(Urls.Collections.collectionById collection.Id)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! databaseState =
                fixture.WordfolioSeeder
                |> Seeder.getAllCollectionsAsync

            Assert.True(databaseState.IsEmpty)
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

            use client = factory.CreateClient()

            let! response = client.DeleteAsync(Urls.Collections.collectionById 1)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
