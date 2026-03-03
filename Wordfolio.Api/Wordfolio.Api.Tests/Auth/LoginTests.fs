namespace Wordfolio.Api.Tests.Auth

open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils

[<CLIMutable>]
type LoginRegisterRequestDto = { Email: string; Password: string }

[<CLIMutable>]
type LoginRequestDto = { Email: string; Password: string }

[<CLIMutable>]
type LoginResponseDto =
    { TokenType: string
      AccessToken: string
      ExpiresIn: int
      RefreshToken: string }

type LoginTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST /auth/login with valid credentials succeeds and returns tokens``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let registerRequest: LoginRegisterRequestDto =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! registerResponse = client.PostAsJsonAsync("/auth/register", registerRequest)
            Assert.True(registerResponse.IsSuccessStatusCode)

            let loginRequest: LoginRequestDto =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)
            let! body = loginResponse.Content.ReadAsStringAsync()

            Assert.True(loginResponse.IsSuccessStatusCode, $"Status: {loginResponse.StatusCode}. Body: {body}")

            let! loginResult = loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>()

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
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let registerRequest: LoginRegisterRequestDto =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! registerResponse = client.PostAsJsonAsync("/auth/register", registerRequest)
            Assert.True(registerResponse.IsSuccessStatusCode)

            let loginRequest: LoginRequestDto =
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
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let loginRequest: LoginRequestDto =
                { Email = "nonexistent@example.com"
                  Password = "P@ssw0rd!" }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)

            Assert.False(loginResponse.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode)
        }
