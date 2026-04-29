namespace Wordfolio.Api.Tests

open System.Net
open System.Net.Http
open System.Text.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests.Utils
open Wordfolio.ServiceDefaults.OpenApi

type OpenApiTests(fixture: WordfolioIdentityTestFixture) =
    let tryGetProperty (element: JsonElement) (propertyName: string) =
        let mutable value =
            Unchecked.defaultof<JsonElement>

        if element.TryGetProperty(propertyName, &value) then
            Some value
        else
            None

    let getRequiredProperties(schema: JsonElement) =
        match tryGetProperty schema "required" with
        | Some required ->
            required.EnumerateArray()
            |> Seq.choose(fun item -> item.GetString() |> Option.ofObj)
            |> Seq.sort
            |> Seq.toList
        | None -> []

    let getStringSet(element: JsonElement) =
        element.EnumerateArray()
        |> Seq.choose(fun item -> item.GetString() |> Option.ofObj)
        |> Seq.sort
        |> Seq.toList

    let getReferenceSet(element: JsonElement) =
        element.EnumerateArray()
        |> Seq.map(fun item -> item.GetProperty("$ref").GetString())
        |> Seq.choose Option.ofObj
        |> Seq.sort
        |> Seq.toList

    let getOpenApiDocumentAsync(client: HttpClient) : Task<JsonDocument> =
        task {
            let! response = client.GetAsync(OpenApiPath)

            response.EnsureSuccessStatusCode()
            |> ignore

            let! content = response.Content.ReadAsStringAsync()
            return JsonDocument.Parse(content)
        }

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
    member _.``Exercises OpenApi contract matches expected request and prompt shapes``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()
            use! document = getOpenApiDocumentAsync client

            let root: JsonElement = document.RootElement

            let paths: JsonElement =
                root.GetProperty("paths")

            let createSessionOperation: JsonElement =
                paths.GetProperty("/exercises/sessions").GetProperty("post")

            let submitAttemptOperation: JsonElement =
                paths
                    .GetProperty("/exercises/sessions/{sessionId}/entries/{entryId}/attempts")
                    .GetProperty("post")

            Assert.True(
                createSessionOperation
                    .GetProperty("requestBody")
                    .GetProperty("required")
                    .GetBoolean()
            )

            Assert.True(
                submitAttemptOperation
                    .GetProperty("requestBody")
                    .GetProperty("required")
                    .GetBoolean()
            )

            let schemas: JsonElement =
                root.GetProperty("components").GetProperty("schemas")

            let entrySelectorRequest: JsonElement =
                schemas.GetProperty("EntrySelectorRequest")

            let requiredProperties =
                getRequiredProperties entrySelectorRequest

            Assert.DoesNotContain("entryIds", requiredProperties)

            let entryIdsTypes =
                entrySelectorRequest
                    .GetProperty("properties")
                    .GetProperty("entryIds")
                    .GetProperty("type")
                |> getStringSet

            Assert.Equal<string list>([ "array"; "null" ], entryIdsTypes)

            let promptDataReferences =
                schemas
                    .GetProperty("SessionBundleEntryResponse")
                    .GetProperty("properties")
                    .GetProperty("promptData")
                    .GetProperty("oneOf")
                |> getReferenceSet

            Assert.Equal<string list>(
                [ "#/components/schemas/MultipleChoicePromptDataResponse"
                  "#/components/schemas/TranslationPromptDataResponse" ],
                promptDataReferences
            )

            let multipleChoicePrompt =
                schemas.GetProperty("MultipleChoicePromptDataResponse")

            let translationPrompt =
                schemas.GetProperty("TranslationPromptDataResponse")

            Assert.Equal<string list>(
                [ "correctOptionId"; "entryText"; "options" ],
                getRequiredProperties multipleChoicePrompt
            )

            Assert.Equal<string list>([ "acceptedTranslations"; "entryText" ], getRequiredProperties translationPrompt)
        }
