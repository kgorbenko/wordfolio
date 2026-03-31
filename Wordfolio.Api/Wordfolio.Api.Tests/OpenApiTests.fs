namespace Wordfolio.Api.Tests

open System.Text.Json
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
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(OpenApiPath)

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("application/json", string response.Content.Headers.ContentType)
        }

    [<Fact>]
    member _.``Swagger UI is accessible without auth``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync("/swagger")

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("text/html", string response.Content.Headers.ContentType)
        }

    [<Fact>]
    member _.``OpenApi document represents optional integer move entry vocabulary id without string``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(OpenApiPath)
            let! body = response.Content.ReadAsStringAsync()

            use document = JsonDocument.Parse(body)

            let vocabularyIdSchema =
                document.RootElement
                    .GetProperty("components")
                    .GetProperty("schemas")
                    .GetProperty("MoveEntryRequest")
                    .GetProperty("properties")
                    .GetProperty("vocabularyId")

            let schemaTypes =
                vocabularyIdSchema.GetProperty("type").EnumerateArray()
                |> Seq.map(fun item -> item.GetString())
                |> Seq.toList

            Assert.Equal<string list>([ "null"; "integer" ], schemaTypes)
        }
