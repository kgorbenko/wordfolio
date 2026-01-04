module Wordfolio.Api.Tests.Infrastructure.WordsApiClientTests

open System.Net
open System.Net.Http
open System.Text
open System.Threading

open Xunit

open Wordfolio.Api.Infrastructure.WordsApi
open Wordfolio.Api.Tests.Infrastructure.FakeHttpMessageHandler

let defaultConfiguration =
    { BaseUrl = "https://wordsapiv1.p.rapidapi.com/words"
      Host = "wordsapiv1.p.rapidapi.com"
      ApiKey = "test-api-key" }

let successResponseJson =
    """
{
    "word": "relax",
    "results": [
        {
            "definition": "make less taut",
            "partOfSpeech": "verb",
            "examples": ["relax the tension on the rope"]
        },
        {
            "definition": "become less tense",
            "partOfSpeech": "verb",
            "examples": ["He relaxed in the hot tub", "Let's all relax"]
        }
    ]
}
"""

let notFoundResponseJson =
    """
{
    "success": false,
    "message": "word not found"
}
"""

[<Fact>]
let ``LookupWordAsync returns definitions with partOfSpeech on success``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(successResponseJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("relax", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Ok
                { Word = "relax"
                  Definitions =
                    [ { Definition = "make less taut"
                        PartOfSpeech = Some "verb"
                        Examples = [ "relax the tension on the rope" ] }
                      { Definition = "become less tense"
                        PartOfSpeech = Some "verb"
                        Examples = [ "He relaxed in the hot tub"; "Let's all relax" ] } ] }

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync deserializes case-insensitive property names``() =
    task {
        let mixedCaseJson =
            """
{
    "Word": "TEST",
    "RESULTS": [
        {
            "DEFINITION": "a test definition",
            "PartOfSpeech": "noun",
            "Examples": ["example one"]
        }
    ]
}
"""

        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mixedCaseJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("test", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Ok
                { Word = "TEST"
                  Definitions =
                    [ { Definition = "a test definition"
                        PartOfSpeech = Some "noun"
                        Examples = [ "example one" ] } ] }

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync sends correct headers``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(successResponseJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let configuration =
            { BaseUrl = "https://wordsapiv1.p.rapidapi.com/words"
              Host = "wordsapiv1.p.rapidapi.com"
              ApiKey = "my-secret-key" }

        let client =
            WordsApiClient(httpClient, configuration)

        let! _ = client.LookupWordAsync("test", CancellationToken.None)

        Assert.Single(handler.RequestHistory)
        |> ignore

        let request = handler.RequestHistory[0]

        Assert.Equal("https://wordsapiv1.p.rapidapi.com/words/test", request.RequestUri.ToString())

        Assert.Equal(
            "wordsapiv1.p.rapidapi.com",
            request.Headers.GetValues("x-rapidapi-host")
            |> Seq.head
        )

        Assert.Equal(
            "my-secret-key",
            request.Headers.GetValues("x-rapidapi-key")
            |> Seq.head
        )
    }

[<Fact>]
let ``LookupWordAsync uses configuration for base URL and host``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(successResponseJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let configuration =
            { BaseUrl = "https://custom-api.example.com/words"
              Host = "custom-api.example.com"
              ApiKey = "custom-key" }

        let client =
            WordsApiClient(httpClient, configuration)

        let! _ = client.LookupWordAsync("test", CancellationToken.None)

        let request = handler.RequestHistory[0]

        Assert.Equal("https://custom-api.example.com/words/test", request.RequestUri.ToString())

        Assert.Equal(
            "custom-api.example.com",
            request.Headers.GetValues("x-rapidapi-host")
            |> Seq.head
        )

        Assert.Equal(
            "custom-key",
            request.Headers.GetValues("x-rapidapi-key")
            |> Seq.head
        )
    }

[<Fact>]
let ``LookupWordAsync escapes special characters in word``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(successResponseJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! _ = client.LookupWordAsync("hello world", CancellationToken.None)

        let request = handler.RequestHistory[0]
        Assert.Equal("https://wordsapiv1.p.rapidapi.com/words/hello%20world", request.RequestUri.OriginalString)
    }

[<Fact>]
let ``LookupWordAsync returns NotFound when word does not exist``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(notFoundResponseJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("xyznonexistent", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Error NotFound

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync returns ApiError on server error``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Internal Server Error", Encoding.UTF8, "text/plain")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("test", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Error(ApiError "HTTP 500: Internal Server Error")

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync returns ApiError on bad gateway``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent("Bad Gateway", Encoding.UTF8, "text/plain")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("test", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Error(ApiError "HTTP 502: Bad Gateway")

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync returns NetworkError on HttpRequestException``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ -> raise(new HttpRequestException("Connection refused")))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("test", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Error(NetworkError "Connection refused")

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync handles response with missing optional fields``() =
    task {
        let minimalResponseJson =
            """
{
    "word": "test",
    "results": [
        {
            "definition": "a procedure for testing"
        }
    ]
}
"""

        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(minimalResponseJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("test", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Ok
                { Word = "test"
                  Definitions =
                    [ { Definition = "a procedure for testing"
                        PartOfSpeech = None
                        Examples = [] } ] }

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync handles response with empty results``() =
    task {
        let emptyResultsJson =
            """
{
    "word": "xyz",
    "results": []
}
"""

        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(emptyResultsJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("xyz", CancellationToken.None)

        let expected: Result<LookupResult, WordsApiError> =
            Ok { Word = "xyz"; Definitions = [] }

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``LookupWordAsync returns ApiError on malformed JSON response``() =
    task {
        let malformedJson = "{ invalid json }"

        use handler =
            new FakeHttpMessageHandler(fun _ ->
                new HttpResponseMessage(
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(malformedJson, Encoding.UTF8, "application/json")
                ))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("test", CancellationToken.None)

        match result with
        | Error(ApiError message) -> Assert.StartsWith("Invalid JSON response:", message)
        | _ -> Assert.Fail("Expected ApiError for malformed JSON")
    }

[<Fact>]
let ``LookupWordAsync returns NetworkError on request cancellation``() =
    task {
        use handler =
            new FakeHttpMessageHandler(fun _ ->
                raise(new System.Threading.Tasks.TaskCanceledException("The request was canceled")))

        use httpClient = new HttpClient(handler)

        let client =
            WordsApiClient(httpClient, defaultConfiguration)

        let! result = client.LookupWordAsync("test", CancellationToken.None)

        match result with
        | Error(NetworkError message) -> Assert.StartsWith("Request timed out:", message)
        | _ -> Assert.Fail("Expected NetworkError for canceled request")
    }
