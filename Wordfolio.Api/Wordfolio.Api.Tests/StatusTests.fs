namespace Wordfolio.Api.Tests

open System.Net
open System.Threading.Tasks

open Xunit

type StatusTests(fixture: FunctionalTestFixture) =
    interface IClassFixture<FunctionalTestFixture>

    [<Fact>]
    member _.``Status endpoint``() : Task =
        task {
            use factory = new WebApplicationFactory(fixture.ConnectionString)
            use client = factory.CreateClient()

            let! response = client.GetAsync("/status")

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        }
