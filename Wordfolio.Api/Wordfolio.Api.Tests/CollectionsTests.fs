namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Handlers.Collections
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

[<CLIMutable>]
type TestRegisterRequest = { Email: string; Password: string }

[<CLIMutable>]
type TestLoginRequest = { Email: string; Password: string }

[<CLIMutable>]
type TestLoginResponse =
    { TokenType: string
      AccessToken: string
      ExpiresIn: int
      RefreshToken: string }

module private TestHelpers =
    let createUserAndGetToken
        (client: HttpClient)
        (fixture: WordfolioIdentityTestFixture)
        (userId: int)
        (email: string)
        (password: string)
        : Task<string> =
        task {
            let identityUser =
                Identity.Seeder.makeUser userId email password

            do!
                fixture.IdentitySeeder
                |> Identity.Seeder.addUsers [ identityUser ]
                |> Identity.Seeder.saveChangesAsync

            let wordfolioUser = Entities.makeUser userId

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            let loginRequest: TestLoginRequest =
                { Email = email; Password = password }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)
            let! loginBody = loginResponse.Content.ReadAsStringAsync()

            Assert.True(
                loginResponse.IsSuccessStatusCode,
                $"Login failed. Status: {loginResponse.StatusCode}. Body: {loginBody}"
            )

            let! loginResult = loginResponse.Content.ReadFromJsonAsync<TestLoginResponse>()
            return loginResult.AccessToken
        }

    let setAuthToken (client: HttpClient) (token: string) : unit =
        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)

type CollectionsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 100 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync(Urls.Collections, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            Assert.NotNull(result)
            Assert.True(result.Id > 0)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let collection = Assert.Single(collections)

            let expected: CollectionResponse =
                { Id = result.Id
                  Name = "My Collection"
                  Description = Some "A test collection"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = None }

            Assert.Equivalent(expected, result)

            Assert.Equivalent(
                { Id = collection.Id
                  UserId = collection.UserId
                  Name = "My Collection"
                  Description = Some "A test collection"
                  CreatedAt = collection.CreatedAt
                  UpdatedAt = None },
                collection
            )
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

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 101 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

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

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 102 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

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

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 103 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let user = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = user.Id
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
            Assert.Equal(savedCollection.Id, result.Id)
            Assert.Equal("Test Collection", result.Name)
            Assert.Equal(Some "Test Description", result.Description)
        }

    [<Fact>]
    member _.``GET by id returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 104 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

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

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 105 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let user = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = user.Id
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
            Assert.Equal(savedCollection.Id, result.Id)
            Assert.Equal("Updated Name", result.Name)
            Assert.Equal(Some "Updated Description", result.Description)
            Assert.True(result.UpdatedAt.IsSome)

            let! updatedCollection = Seeder.getCollectionByIdAsync savedCollection.Id fixture.WordfolioSeeder
            Assert.True(updatedCollection.IsSome)
            Assert.Equal("Updated Name", updatedCollection.Value.Name)
            Assert.Equal(Some "Updated Description", updatedCollection.Value.Description)
        }

    [<Fact>]
    member _.``PUT returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 106 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

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

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 107 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let user = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = user.Id
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

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 108 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

            let! users = Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let user = Assert.Single(users)

            let collection: Mapping.Collection =
                { Id = 0
                  UserId = user.Id
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

            use client = factory.CreateClient()

            let! token = TestHelpers.createUserAndGetToken client fixture 109 "user@example.com" "P@ssw0rd!"
            TestHelpers.setAuthToken client token

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
