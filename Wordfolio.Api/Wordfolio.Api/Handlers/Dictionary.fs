module Wordfolio.Api.Handlers.Dictionary

open System
open System.Collections.Generic
open System.Text.Json.Nodes

open FSharp.Control

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Wordfolio.Api.Configuration.GroqApi
open Wordfolio.GroqApi

module Urls = Wordfolio.Api.Urls.Dictionary

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
        Func<string, GroqApiConfiguration, IResult>(fun text groqConfig ->
            if String.IsNullOrWhiteSpace(text) then
                Results.BadRequest({| error = "Text parameter is required" |})
            else
                Results.Ok(streamLookup groqConfig text))
    )
    |> ignore
