namespace Wordfolio.Api.Tests

open System.Net
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests.Utils
open Wordfolio.ServiceDefaults.Status

type StatusTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``Status endpoint``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.GetAsync(StatusUrl)

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        }
