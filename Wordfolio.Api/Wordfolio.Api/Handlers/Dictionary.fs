module Wordfolio.Api.Handlers.Dictionary

open System
open System.Collections.Generic
open System.Text.Json.Nodes
open System.Threading

open FSharp.Control

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Wordfolio.Api.Configuration.GroqApi
open Wordfolio.Api.Infrastructure.WordsApi
open Wordfolio.GroqApi

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Dictionary

type ExampleResponse = { Text: string }

type DefinitionResponse =
    { DefinitionText: string
      PartOfSpeech: string
      Examples: ExampleResponse list }

type TranslationResponse =
    { TranslationText: string
      Examples: ExampleResponse list }

type LookupResponse =
    { Text: string
      Definitions: DefinitionResponse list
      Translations: TranslationResponse list }

type LookupErrorResponse = { Error: string }

let private mapLookupResult(result: LookupResult) : LookupResponse =
    let definitions =
        result.Definitions
        |> List.map(fun def ->
            { DefinitionText = def.Definition
              PartOfSpeech =
                def.PartOfSpeech
                |> Option.defaultValue ""
              Examples =
                def.Examples
                |> List.map(fun ex -> { Text = ex }) })

    { Text = result.Word
      Definitions = definitions
      Translations = [] }

let private createWordLookupPrompt(word: string) =
    $"""For the English word/phrase "{word}", provide definitions and translations to Russian in the following JSON format:

{{
  "definitions": [
    {{
      "definition": "...",
      "partOfSpeech": "noun|verb|adj|...",
      "exampleSentences": [ "...", "..." ]
    }}
  ],
  "translations": [
    {{
      "translation": "...",
      "partOfSpeech": "noun|verb|adj|...",
      "examples": [
        {{ "russian": "...", "english": "..." }}
      ]
    }}
  ]
}}

Tend to make definitions under 10 words each. Tend to make example sentences under 15 words each. Provide 1-2 example sentences per definition and translation. Highlight requested word/phrase in the example sentences with asterisks.

Provide only the JSON response. Do not change JSON format. Do not include any explanations or additional text."""

let private createChatRequest(model: string, prompt: string) =
    let request = JsonObject()
    request["model"] <- model

    let userMessage = JsonObject()
    userMessage["role"] <- "user"
    userMessage["content"] <- prompt

    let messages = JsonArray()
    messages.Add(userMessage)

    request["messages"] <- messages
    request["temperature"] <- 0.1
    request["max_tokens"] <- 2048
    request

let private extractContent(chunk: JsonNode) : string option =
    let tryGetProperty (propertyGetter: JsonNode -> JsonNode) (obj: JsonNode option) : JsonNode option =
        obj
        |> Option.bind(fun x -> propertyGetter x |> Option.ofObj)

    chunk
    |> Option.ofObj
    |> tryGetProperty(fun x -> x["choices"])
    |> tryGetProperty(fun x -> x[0])
    |> tryGetProperty(fun x -> x["delta"])
    |> tryGetProperty(fun x -> x["content"])
    |> Option.map _.GetValue<string>()

let private streamLookup (groqConfig: GroqApiConfiguration) (text: string) : IAsyncEnumerable<string> =
    taskSeq {
        let prompt = createWordLookupPrompt text

        let request =
            createChatRequest(groqConfig.Model, prompt)

        use groqClient =
            new GroqApiClient(groqConfig.ApiKey)

        for chunk in groqClient.CreateChatCompletionStreamAsync(request) do
            match extractContent chunk with
            | Some contentText -> yield contentText
            | None -> ()
    }

let mapDictionaryEndpoints(group: RouteGroupBuilder) =
    group.MapGet(
        Urls.Lookup,
        Func<string, WordsApiClient, CancellationToken, _>(fun text wordsApiClient cancellationToken ->
            task {
                if String.IsNullOrWhiteSpace(text) then
                    return Results.BadRequest({| error = "Text parameter is required" |})
                else
                    let! result = wordsApiClient.LookupWordAsync(text, cancellationToken)

                    match result with
                    | Ok lookupResult ->
                        let response = mapLookupResult lookupResult
                        return Results.Ok(response)
                    | Error NotFound ->
                        return Results.NotFound({ Error = $"No definitions found for '{text}'" }: LookupErrorResponse)
                    | Error(ApiError message) -> return Results.Problem(message, statusCode = 502)
                    | Error(NetworkError message) -> return Results.Problem(message, statusCode = 503)
            })
    )
    |> ignore

    group.MapGet(
        Urls.LookupStream,
        Func<string, GroqApiConfiguration, IResult>(fun text groqConfig ->
            if String.IsNullOrWhiteSpace(text) then
                Results.BadRequest({| error = "Text parameter is required" |})
            else
                Results.Ok(streamLookup groqConfig text))
    )
    |> ignore
