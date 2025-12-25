namespace Wordfolio.Api.Tests

open System.Net
open System.Text.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests.Utils
open Wordfolio.ServiceDefaults.OpenApi

type OpenApiTagsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``OpenApi document contains Collections tag``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(OpenApiPath)
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("Collections", content)
        }

    [<Fact>]
    member _.``OpenApi document contains Vocabularies tag``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(OpenApiPath)
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("Vocabularies", content)
        }

    [<Fact>]
    member _.``OpenApi document contains Auth tag``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(OpenApiPath)
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("Auth", content)
        }
