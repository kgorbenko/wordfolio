namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Handlers.Collections
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module private TestHelpers =
    let createUserAsync
        (factory: WebApplicationFactory)
        (userId: int)
        (email: string)
        (password: string)
        : Task<Wordfolio.Api.Identity.User> =
        task {
            use scope = factory.Services.CreateScope()

            let userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<Wordfolio.Api.Identity.User>>()

            let identityUser =
                Wordfolio.Api.Identity.User(Id = userId, UserName = email, Email = email)

            let! createResult = userManager.CreateAsync(identityUser, password)

            if not createResult.Succeeded then
                let errors =
                    String.concat
                        ", "
                        (createResult.Errors
                         |> Seq.map(fun e -> e.Description))

                Assert.Fail($"User creation failed: {errors}")

            return identityUser
        }

type CollectionsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 100 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()
            Assert.NotNull(result)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let collection = Assert.Single(collections)

            let expected: Wordfolio.Collection =
                { Id = collection.Id
                  UserId = 100
                  Name = "My Collection"
                  Description = Some "A test collection"
                  CreatedAt = collection.CreatedAt
                  UpdatedAt = None }

            Assert.Equal(expected, collection)
        }

    [<Fact>]
    member _.``POST without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 101 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let request: CreateCollectionRequest =
                { Name = ""
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``GET returns empty list when no collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 102 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let! response = client.GetAsync(Urls.Collections)
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
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.Collections)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns specific collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 103 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let wordfolioUser = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = wordfolioUser.Id
                  Name = "Test Collection"
                  Description = Some "Test Description" |> Option.toObj
                  CreatedAt = DateTimeOffset.UtcNow
                  UpdatedAt = None |> Option.toNullable
                  User = Unchecked.defaultof<_>
                  Vocabularies = ResizeArray() }

            do!
                fixture.WordfolioSeeder
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            let savedCollection =
                Assert.Single(collections)

            let! response = client.GetAsync(Urls.CollectionById savedCollection.Id)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            Assert.NotNull(result)

            let expected: CollectionResponse =
                { Id = savedCollection.Id
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
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 104 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let! response = client.GetAsync(Urls.CollectionById 999999)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.CollectionById 1)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT updates collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 105 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let wordfolioUser = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = wordfolioUser.Id
                  Name = "Original Name"
                  Description =
                    Some "Original Description"
                    |> Option.toObj
                  CreatedAt = DateTimeOffset.UtcNow
                  UpdatedAt = None |> Option.toNullable
                  User = Unchecked.defaultof<_>
                  Vocabularies = ResizeArray() }

            do!
                fixture.WordfolioSeeder
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            let savedCollection =
                Assert.Single(collections)

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.CollectionById savedCollection.Id, updateRequest)
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
                  UpdatedAt = collection.UpdatedAt }

            Assert.Equal(expected, collection)
        }

    [<Fact>]
    member _.``PUT returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 106 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.CollectionById 999999, updateRequest)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 107 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let wordfolioUser = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = wordfolioUser.Id
                  Name = "Original Name"
                  Description = None |> Option.toObj
                  CreatedAt = DateTimeOffset.UtcNow
                  UpdatedAt = None |> Option.toNullable
                  User = Unchecked.defaultof<_>
                  Vocabularies = ResizeArray() }

            do!
                fixture.WordfolioSeeder
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            let savedCollection =
                Assert.Single(collections)

            let updateRequest: UpdateCollectionRequest =
                { Name = ""
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.CollectionById savedCollection.Id, updateRequest)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync(Urls.CollectionById 1, updateRequest)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE deletes collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! identityUser = TestHelpers.createUserAsync factory 108 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let wordfolioUser = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = wordfolioUser.Id
                  Name = "Test Collection"
                  Description = None |> Option.toObj
                  CreatedAt = DateTimeOffset.UtcNow
                  UpdatedAt = None |> Option.toNullable
                  User = Unchecked.defaultof<_>
                  Vocabularies = ResizeArray() }

            do!
                fixture.WordfolioSeeder
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder

            let savedCollection =
                Assert.Single(collections)

            let! response = client.DeleteAsync(Urls.CollectionById savedCollection.Id)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! deletedCollection = Seeder.getCollectionByIdAsync savedCollection.Id fixture.WordfolioSeeder
            Assert.True(deletedCollection.IsNone)
        }

    [<Fact>]
    member _.``DELETE returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            let! user = TestHelpers.createUserAsync factory 109 "user@example.com" "P@ssw0rd!"
            use! client = factory.CreateAuthenticatedClientAsync(user)

            let! response = client.DeleteAsync(Urls.CollectionById 999999)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.DeleteAsync(Urls.CollectionById 1)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
