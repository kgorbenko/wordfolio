namespace Wordfolio.Api.Tests.Auth

open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils

type PasswordRequirementsResponse =
    { RequiredLength: int
      RequireDigit: bool
      RequireLowercase: bool
      RequireUppercase: bool
      RequireNonAlphanumeric: bool
      RequiredUniqueChars: int }

type GetPasswordRequirementsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET /auth/password-requirements returns password requirements``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.Auth.passwordRequirements())
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! requirements = response.Content.ReadFromJsonAsync<PasswordRequirementsResponse>()

            let expected: PasswordRequirementsResponse =
                { RequiredLength = 6
                  RequireDigit = true
                  RequireLowercase = true
                  RequireUppercase = true
                  RequireNonAlphanumeric = true
                  RequiredUniqueChars = 1 }

            Assert.Equal(expected, requirements)
        }
