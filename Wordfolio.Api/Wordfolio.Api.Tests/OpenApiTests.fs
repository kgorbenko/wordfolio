namespace Wordfolio.Api.Tests

open System.Net
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests.Utils
open Wordfolio.ServiceDefaults.OpenApi

type OpenApiTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``OpenApi document is accessible without auth``() : Task =
        task {
            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.GetAsync(OpenApiPath)

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("application/json", string response.Content.Headers.ContentType)
        }

    [<Fact>]
    member _.``Swagger UI is accessible without auth``() : Task =
        task {
            use factory =
                new WebApplicationFactory(fixture.ConnectionString)

            use client = factory.CreateClient()

            let! response = client.GetAsync("/swagger")

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("text/html", string response.Content.Headers.ContentType)
        }
