namespace Wordfolio.Api.Tests.Auth

open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils

[<CLIMutable>]
type ManageInfoResponse =
    { Email: string
      IsEmailConfirmed: bool }

type GetManageInfoTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET /auth/manage/info returns authenticated user email``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, _ = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Auth.manageInfo())
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<ManageInfoResponse>()

            let expected: ManageInfoResponse =
                { Email = "user@example.com"
                  IsEmailConfirmed = false }

            Assert.Equal(expected, result)
        }

    [<Fact>]
    member _.``GET /auth/manage/info without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.Auth.manageInfo())

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
