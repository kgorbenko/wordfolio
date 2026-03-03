namespace Wordfolio.Api.Tests.Auth

open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils

[<CLIMutable>]
type RefreshRegisterRequestDto = { Email: string; Password: string }

[<CLIMutable>]
type RefreshLoginRequestDto = { Email: string; Password: string }

[<CLIMutable>]
type RefreshLoginResponseDto =
    { TokenType: string
      AccessToken: string
      ExpiresIn: int
      RefreshToken: string }

[<CLIMutable>]
type RefreshRequestDto = { RefreshToken: string }

[<CLIMutable>]
type RefreshResponseDto =
    { TokenType: string
      AccessToken: string
      ExpiresIn: int
      RefreshToken: string }

type RefreshTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST /auth/refresh with valid token succeeds and returns new tokens``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let registerRequest: RefreshRegisterRequestDto =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! registerResponse = client.PostAsJsonAsync("/auth/register", registerRequest)
            Assert.True(registerResponse.IsSuccessStatusCode)

            let loginRequest: RefreshLoginRequestDto =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! loginResponse = client.PostAsJsonAsync("/auth/login", loginRequest)
            Assert.True(loginResponse.IsSuccessStatusCode)

            let! loginResult = loginResponse.Content.ReadFromJsonAsync<RefreshLoginResponseDto>()

            let refreshRequest: RefreshRequestDto =
                { RefreshToken = loginResult.RefreshToken }

            let! refreshResponse = client.PostAsJsonAsync("/auth/refresh", refreshRequest)
            let! body = refreshResponse.Content.ReadAsStringAsync()

            Assert.True(refreshResponse.IsSuccessStatusCode, $"Status: {refreshResponse.StatusCode}. Body: {body}")

            let! refreshResult = refreshResponse.Content.ReadFromJsonAsync<RefreshResponseDto>()

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
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let refreshRequest: RefreshRequestDto =
                { RefreshToken = "invalid-token" }

            let! refreshResponse = client.PostAsJsonAsync("/auth/refresh", refreshRequest)

            Assert.False(refreshResponse.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode)
        }
