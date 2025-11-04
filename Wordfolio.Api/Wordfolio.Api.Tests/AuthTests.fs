namespace Wordfolio.Api.Tests

open System.Collections.Generic
open System.Net
open System.Net.Http.Json
open System.Text.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

[<CLIMutable>]
type RegisterRequest = { Email: string; Password: string }

[<CLIMutable>]
type LoginRequest = { Email: string; Password: string }

[<CLIMutable>]
type LoginResponse =
    { TokenType: string
      AccessToken: string
      ExpiresIn: int
      RefreshToken: string }

[<CLIMutable>]
type RefreshRequest = { RefreshToken: string }

[<CLIMutable>]
type RefreshResponse =
    { TokenType: string
      AccessToken: string
      ExpiresIn: int
      RefreshToken: string }

[<CLIMutable>]
type ValidationProblemDetails =
    { Type: string
      Title: string
      Status: int
      Errors: Dictionary<string, string[]> }

type AuthTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST /auth/register succeeds and creates rows in both schemas``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let request: RegisterRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! response = client.PostAsJsonAsync("/auth/register", request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actualWordfolioUsers = Wordfolio.Seeder.getAllUsersAsync fixture.WordfolioSeeder
            let! actualIdentityUsers = Identity.Seeder.getAllUsersAsync fixture.IdentitySeeder

            let wordfolioUser =
                Assert.Single(actualWordfolioUsers)

            let identityUser =
                Assert.Single(actualIdentityUsers)

            Assert.Equal(request.Email, identityUser.Email)
            Assert.Equal(wordfolioUser.Id, identityUser.Id)
        }

    [<Fact>]
    member _.``POST /auth/register with duplicate email fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let request: RegisterRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! firstResponse = client.PostAsJsonAsync("/auth/register", request)
            Assert.True(firstResponse.IsSuccessStatusCode)

            let! secondResponse = client.PostAsJsonAsync("/auth/register", request)
            Assert.False(secondResponse.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode)

            let! errorContent = secondResponse.Content.ReadAsStringAsync()

            let errorJson =
                JsonDocument.Parse(errorContent)

            let root = errorJson.RootElement

            let mutable errorsProperty =
                Unchecked.defaultof<JsonElement>

            Assert.True(root.TryGetProperty("errors", &errorsProperty))

            let mutable duplicateUserProperty =
                Unchecked.defaultof<JsonElement>

            Assert.True(errorsProperty.TryGetProperty("DuplicateUserName", &duplicateUserProperty))
        }

    [<Fact>]
    member _.``POST /auth/register with weak password fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let request: RegisterRequest =
                { Email = "user@example.com"
                  Password = "weak" }

            let! response = client.PostAsJsonAsync("/auth/register", request)
            Assert.False(response.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! errorContent = response.Content.ReadAsStringAsync()

            let errorJson =
                JsonDocument.Parse(errorContent)

            let root = errorJson.RootElement

            let mutable errorsProperty =
                Unchecked.defaultof<JsonElement>

            Assert.True(root.TryGetProperty("errors", &errorsProperty))

            let mutable passwordProperty =
                Unchecked.defaultof<JsonElement>

            Assert.True(errorsProperty.TryGetProperty("PasswordTooShort", &passwordProperty))
        }

    [<Fact>]
    member _.``POST /auth/login with valid credentials succeeds and returns tokens``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let registerRequest: RegisterRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! registerResponse = client.PostAsJsonAsync("/auth/register", registerRequest)
            Assert.True(registerResponse.IsSuccessStatusCode)

            let loginRequest: LoginRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)
            let! body = loginResponse.Content.ReadAsStringAsync()

            Assert.True(loginResponse.IsSuccessStatusCode, $"Status: {loginResponse.StatusCode}. Body: {body}")

            let! loginResult = loginResponse.Content.ReadFromJsonAsync<LoginResponse>()

            Assert.NotNull(loginResult)
            Assert.Equal("Bearer", loginResult.TokenType)
            Assert.NotEmpty(loginResult.AccessToken)
            Assert.True(loginResult.ExpiresIn > 0)
            Assert.NotEmpty(loginResult.RefreshToken)
        }

    [<Fact>]
    member _.``POST /auth/login with invalid credentials fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let registerRequest: RegisterRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! registerResponse = client.PostAsJsonAsync("/auth/register", registerRequest)
            Assert.True(registerResponse.IsSuccessStatusCode)

            let loginRequest: LoginRequest =
                { Email = "user@example.com"
                  Password = "WrongPassword123!" }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)

            Assert.False(loginResponse.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode)
        }

    [<Fact>]
    member _.``POST /auth/login with non-existent user fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let loginRequest: LoginRequest =
                { Email = "nonexistent@example.com"
                  Password = "P@ssw0rd!" }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)

            Assert.False(loginResponse.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode)
        }

    [<Fact>]
    member _.``POST /auth/refresh with valid token succeeds and returns new tokens``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let registerRequest: RegisterRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! registerResponse = client.PostAsJsonAsync("/auth/register", registerRequest)
            Assert.True(registerResponse.IsSuccessStatusCode)

            let loginRequest: LoginRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)
            Assert.True(loginResponse.IsSuccessStatusCode)

            let! loginResult = loginResponse.Content.ReadFromJsonAsync<LoginResponse>()

            let refreshRequest: RefreshRequest =
                { RefreshToken = loginResult.RefreshToken }

            let! refreshResponse = client.PostAsJsonAsync("/auth/refresh", refreshRequest)
            let! body = refreshResponse.Content.ReadAsStringAsync()

            Assert.True(refreshResponse.IsSuccessStatusCode, $"Status: {refreshResponse.StatusCode}. Body: {body}")

            let! refreshResult = refreshResponse.Content.ReadFromJsonAsync<RefreshResponse>()

            Assert.NotNull(refreshResult)
            Assert.Equal("Bearer", refreshResult.TokenType)
            Assert.NotEmpty(refreshResult.AccessToken)
            Assert.True(refreshResult.ExpiresIn > 0)
            Assert.NotEmpty(refreshResult.RefreshToken)
            Assert.NotEqual<string>(loginResult.AccessToken, refreshResult.AccessToken)
        }

    [<Fact>]
    member _.``POST /auth/refresh with invalid token fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let refreshRequest: RefreshRequest =
                { RefreshToken = "invalid-token" }

            let! refreshResponse = client.PostAsJsonAsync("/auth/refresh", refreshRequest)

            Assert.False(refreshResponse.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode)
        }
