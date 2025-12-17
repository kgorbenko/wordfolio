namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Net.Http.Json
open System.Text.Json.Serialization
open System.Threading.Tasks

open Xunit

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

[<CLIMutable>]
type CollectionResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

[<CLIMutable>]
type CreateCollectionRequest =
    { Name: string
      Description: string option }

[<CLIMutable>]
type UpdateCollectionRequest =
    { Name: string
      Description: string option }

module private CollectionsTestHelpers =
    let registerAndLogin (client: HttpClient) (email: string) (password: string) : Task<string> =
        task {
            let registerRequest: TestRegisterRequest =
                { Email = email; Password = password }

            let! registerResponse = client.PostAsJsonAsync("/auth/register", registerRequest)
            let! registerBody = registerResponse.Content.ReadAsStringAsync()

            Assert.True(
                registerResponse.IsSuccessStatusCode,
                $"Register failed. Status: {registerResponse.StatusCode}. Body: {registerBody}"
            )

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
    member _.``POST /collections creates a new collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync("/collections", request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            Assert.NotNull(result)
            Assert.True(result.Id > 0)
            Assert.Equal("My Collection", result.Name)
            Assert.Equal(Some "A test collection", result.Description)

            Assert.True(
                result.CreatedAt
                <= DateTimeOffset.UtcNow
            )

            Assert.Equal(None, result.UpdatedAt)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let collection = Assert.Single(collections)
            Assert.Equal(result.Id, collection.Id)
            Assert.Equal(result.Name, collection.Name)
        }

    [<Fact>]
    member _.``POST /collections without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let request: CreateCollectionRequest =
                { Name = "My Collection"
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync("/collections", request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST /collections with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            let request: CreateCollectionRequest =
                { Name = ""
                  Description = Some "A test collection" }

            let! response = client.PostAsJsonAsync("/collections", request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /collections returns user's collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            // Create collections via API
            let request1: CreateCollectionRequest =
                { Name = "Collection 1"
                  Description = Some "Description 1" }

            let! createResponse1 = client.PostAsJsonAsync("/collections", request1)
            Assert.True(createResponse1.IsSuccessStatusCode)

            let request2: CreateCollectionRequest =
                { Name = "Collection 2"
                  Description = None }

            let! createResponse2 = client.PostAsJsonAsync("/collections", request2)
            Assert.True(createResponse2.IsSuccessStatusCode)

            let! response = client.GetAsync("/collections")
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse[]>()

            Assert.NotNull(result)
            Assert.Equal(2, result.Length)
        }

    [<Fact>]
    member _.``GET /collections returns empty list when no collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            let! response = client.GetAsync("/collections")
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse[]>()

            Assert.NotNull(result)
            Assert.Empty(result)
        }

    [<Fact>]
    member _.``GET /collections without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.GetAsync("/collections")

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /collections returns only authenticated user's collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token1 = CollectionsTestHelpers.registerAndLogin client "user1@example.com" "P@ssw0rd!"
            let! token2 = CollectionsTestHelpers.registerAndLogin client "user2@example.com" "P@ssw0rd!"

            // Create collection for user1
            CollectionsTestHelpers.setAuthToken client token1

            let request1: CreateCollectionRequest =
                { Name = "User1 Collection"
                  Description = None }

            let! createResponse1 = client.PostAsJsonAsync("/collections", request1)
            Assert.True(createResponse1.IsSuccessStatusCode)

            // Create collection for user2
            CollectionsTestHelpers.setAuthToken client token2

            let request2: CreateCollectionRequest =
                { Name = "User2 Collection"
                  Description = None }

            let! createResponse2 = client.PostAsJsonAsync("/collections", request2)
            Assert.True(createResponse2.IsSuccessStatusCode)

            // Verify user1 only sees their collection
            CollectionsTestHelpers.setAuthToken client token1
            let! response1 = client.GetAsync("/collections")
            Assert.True(response1.IsSuccessStatusCode)

            let! result1 = response1.Content.ReadFromJsonAsync<CollectionResponse[]>()
            Assert.Single(result1) |> ignore
            Assert.Equal("User1 Collection", result1.[0].Name)

            // Verify user2 only sees their collection
            CollectionsTestHelpers.setAuthToken client token2
            let! response2 = client.GetAsync("/collections")
            Assert.True(response2.IsSuccessStatusCode)

            let! result2 = response2.Content.ReadFromJsonAsync<CollectionResponse[]>()
            Assert.Single(result2) |> ignore
            Assert.Equal("User2 Collection", result2.[0].Name)
        }

    [<Fact>]
    member _.``GET /collections/{id} returns specific collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            // Create collection via API
            let request: CreateCollectionRequest =
                { Name = "Test Collection"
                  Description = Some "Test Description" }

            let! createResponse = client.PostAsJsonAsync("/collections", request)
            Assert.True(createResponse.IsSuccessStatusCode)

            let! createdCollection = createResponse.Content.ReadFromJsonAsync<CollectionResponse>()

            let! response = client.GetAsync($"/collections/{createdCollection.Id}")
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            Assert.NotNull(result)
            Assert.Equal(createdCollection.Id, result.Id)
            Assert.Equal("Test Collection", result.Name)
            Assert.Equal(Some "Test Description", result.Description)
        }

    [<Fact>]
    member _.``GET /collections/{id} returns 404 when collection doesn't exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            let! response = client.GetAsync("/collections/999999")

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /collections/{id} returns 403 when accessing another user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token1 = CollectionsTestHelpers.registerAndLogin client "user1@example.com" "P@ssw0rd!"
            let! token2 = CollectionsTestHelpers.registerAndLogin client "user2@example.com" "P@ssw0rd!"

            // Create collection as user1
            CollectionsTestHelpers.setAuthToken client token1

            let request: CreateCollectionRequest =
                { Name = "User1 Collection"
                  Description = None }

            let! createResponse = client.PostAsJsonAsync("/collections", request)
            Assert.True(createResponse.IsSuccessStatusCode)

            let! createdCollection = createResponse.Content.ReadFromJsonAsync<CollectionResponse>()

            // Try to access user1's collection as user2
            CollectionsTestHelpers.setAuthToken client token2
            let! response = client.GetAsync($"/collections/{createdCollection.Id}")

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /collections/{id} without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.GetAsync("/collections/1")

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT /collections/{id} updates collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            // Create collection via API
            let createRequest: CreateCollectionRequest =
                { Name = "Original Name"
                  Description = Some "Original Description" }

            let! createResponse = client.PostAsJsonAsync("/collections", createRequest)
            Assert.True(createResponse.IsSuccessStatusCode)

            let! createdCollection = createResponse.Content.ReadFromJsonAsync<CollectionResponse>()

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync($"/collections/{createdCollection.Id}", updateRequest)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<CollectionResponse>()

            Assert.NotNull(result)
            Assert.Equal(createdCollection.Id, result.Id)
            Assert.Equal("Updated Name", result.Name)
            Assert.Equal(Some "Updated Description", result.Description)
            Assert.True(result.UpdatedAt.IsSome)

            let! updatedCollection = Seeder.getCollectionByIdAsync createdCollection.Id fixture.WordfolioSeeder
            Assert.True(updatedCollection.IsSome)
            Assert.Equal("Updated Name", updatedCollection.Value.Name)
            Assert.Equal(Some "Updated Description", updatedCollection.Value.Description)
        }

    [<Fact>]
    member _.``PUT /collections/{id} returns 404 when collection doesn't exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync("/collections/999999", updateRequest)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT /collections/{id} returns 403 when updating another user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token1 = CollectionsTestHelpers.registerAndLogin client "user1@example.com" "P@ssw0rd!"
            let! token2 = CollectionsTestHelpers.registerAndLogin client "user2@example.com" "P@ssw0rd!"

            // Create collection as user1
            CollectionsTestHelpers.setAuthToken client token1

            let createRequest: CreateCollectionRequest =
                { Name = "User1 Collection"
                  Description = None }

            let! createResponse = client.PostAsJsonAsync("/collections", createRequest)
            Assert.True(createResponse.IsSuccessStatusCode)

            let! createdCollection = createResponse.Content.ReadFromJsonAsync<CollectionResponse>()

            // Try to update user1's collection as user2
            CollectionsTestHelpers.setAuthToken client token2

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync($"/collections/{createdCollection.Id}", updateRequest)

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT /collections/{id} with empty name fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            // Create collection via API
            let createRequest: CreateCollectionRequest =
                { Name = "Original Name"
                  Description = None }

            let! createResponse = client.PostAsJsonAsync("/collections", createRequest)
            Assert.True(createResponse.IsSuccessStatusCode)

            let! createdCollection = createResponse.Content.ReadFromJsonAsync<CollectionResponse>()

            let updateRequest: UpdateCollectionRequest =
                { Name = ""
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync($"/collections/{createdCollection.Id}", updateRequest)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT /collections/{id} without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let updateRequest: UpdateCollectionRequest =
                { Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! response = client.PutAsJsonAsync("/collections/1", updateRequest)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE /collections/{id} deletes collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            // Create collection via API
            let createRequest: CreateCollectionRequest =
                { Name = "Test Collection"
                  Description = None }

            let! createResponse = client.PostAsJsonAsync("/collections", createRequest)
            Assert.True(createResponse.IsSuccessStatusCode)

            let! createdCollection = createResponse.Content.ReadFromJsonAsync<CollectionResponse>()

            let! response = client.DeleteAsync($"/collections/{createdCollection.Id}")
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! deletedCollection = Seeder.getCollectionByIdAsync createdCollection.Id fixture.WordfolioSeeder
            Assert.True(deletedCollection.IsNone)
        }

    [<Fact>]
    member _.``DELETE /collections/{id} returns 404 when collection doesn't exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token = CollectionsTestHelpers.registerAndLogin client "user@example.com" "P@ssw0rd!"
            CollectionsTestHelpers.setAuthToken client token

            let! response = client.DeleteAsync("/collections/999999")

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE /collections/{id} returns 403 when deleting another user's collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! token1 = CollectionsTestHelpers.registerAndLogin client "user1@example.com" "P@ssw0rd!"
            let! token2 = CollectionsTestHelpers.registerAndLogin client "user2@example.com" "P@ssw0rd!"

            // Create collection as user1
            CollectionsTestHelpers.setAuthToken client token1

            let createRequest: CreateCollectionRequest =
                { Name = "User1 Collection"
                  Description = None }

            let! createResponse = client.PostAsJsonAsync("/collections", createRequest)
            Assert.True(createResponse.IsSuccessStatusCode)

            let! createdCollection = createResponse.Content.ReadFromJsonAsync<CollectionResponse>()

            // Try to delete user1's collection as user2
            CollectionsTestHelpers.setAuthToken client token2
            let! response = client.DeleteAsync($"/collections/{createdCollection.Id}")

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE /collections/{id} without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.DeleteAsync("/collections/1")

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
