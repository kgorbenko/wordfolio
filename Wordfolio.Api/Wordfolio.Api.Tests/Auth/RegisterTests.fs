namespace Wordfolio.Api.Tests.Auth

open System.Collections.Generic
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

[<CLIMutable>]
type RegisterRequestDto = { Email: string; Password: string }

[<CLIMutable>]
type RegisterValidationProblemDetails =
    { Type: string
      Title: string
      Status: int
      Errors: Dictionary<string, string[]> }

type RegisterTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST /auth/register succeeds and creates rows in both schemas``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: RegisterRequestDto =
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
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: RegisterRequestDto =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! firstResponse = client.PostAsJsonAsync("/auth/register", request)
            Assert.True(firstResponse.IsSuccessStatusCode)

            let! secondResponse = client.PostAsJsonAsync("/auth/register", request)
            Assert.False(secondResponse.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode)

            let! validationError = secondResponse.Content.ReadFromJsonAsync<RegisterValidationProblemDetails>()

            Assert.NotNull(validationError)
            Assert.True(validationError.Errors.ContainsKey("DuplicateUserName"))
        }

    [<Fact>]
    member _.``POST /auth/register with weak password fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: RegisterRequestDto =
                { Email = "user@example.com"
                  Password = "weak" }

            let! response = client.PostAsJsonAsync("/auth/register", request)
            Assert.False(response.IsSuccessStatusCode)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! validationError = response.Content.ReadFromJsonAsync<RegisterValidationProblemDetails>()

            Assert.NotNull(validationError)
            Assert.True(validationError.Errors.ContainsKey("PasswordTooShort"))
        }
