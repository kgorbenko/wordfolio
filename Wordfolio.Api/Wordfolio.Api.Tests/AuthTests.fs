namespace Wordfolio.Api.Tests

open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

[<CLIMutable>]
type RegisterRequest = { Email: string; Password: string }

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
