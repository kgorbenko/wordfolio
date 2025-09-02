namespace Wordfolio.Api.Tests

open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess.Tests

[<CLIMutable>]
type RegisterRequest =
    { Email: string
      Password: string }

type RegisterTests(fixture: FunctionalTestFixture) =
    interface IClassFixture<FunctionalTestFixture>

    [<Fact>]
    member _.``POST /auth/register succeeds and creates a wordfolio user row``() : Task =
        task {
            use factory = new WebApplicationFactory(fixture.ConnectionString)
            use client = factory.CreateClient()

            let request: RegisterRequest =
                { Email = "user@example.com"
                  Password = "P@ssw0rd!" }

            let! response = client.PostAsJsonAsync("/auth/register", request)

            do response.EnsureSuccessStatusCode() |> ignore

            let! actualWordfolioUsers = DatabaseSeeder.getAllUsersAsync fixture.Seeder

            Assert.Single(actualWordfolioUsers)
            |> ignore
        }