module Wordfolio.Api.Handlers.Dictionary

open System
open System.Net.ServerSentEvents
open System.Threading

open FSharp.Control

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Wordfolio.Api.Infrastructure.ChatClient
open Wordfolio.Api.Infrastructure.DelimitedStreamProcessor

module Urls = Wordfolio.Api.Urls.Dictionary

[<Literal>]
let private JsonDelimiter = "---JSON---"

let private createWordLookupPrompt(text: string) =
    $"""For the English word or phrase "{text}", provide definitions and Russian translations.

Output in two parts separated by the exact marker {JsonDelimiter}

First part - clean readable text for end users. Format:

[part of speech] Definition text here.
"Example sentence with *{text}* highlighted."

[part of speech] Перевод на русский
RU: "Пример на русском с *переводом*."
EN: "English translation of the example."

For phrases, omit [part of speech]:
Definition of the phrase.
"Example with *the phrase* in use."

{JsonDelimiter}

Second part - raw JSON only (no code fences):
{{"definitions":[{{"definition":"...","partOfSpeech":"verb|noun|adj|adv|null","exampleSentences":["..."]}}],"translations":[{{"translation":"...","partOfSpeech":"verb|noun|adj|adv|null","examples":[{{"russian":"...","english":"..."}}]}}]}}

Rules:
- Provide the most common definitions and translations (up to 5 each)
- For single words, include partOfSpeech (verb, noun, adj, adv)
- For phrases, set partOfSpeech to null and omit [part of speech] prefix
- Definitions under 10 words each
- Example sentences under 15 words each
- 1-2 examples per definition/translation
- Highlight "{text}" with asterisks in examples (e.g., *{text}*)
- No markdown formatting, no headers, no extra text
- Blank line between each definition and translation entry
- JSON must be valid and compact (single line, no pretty-printing)"""

let private streamLookup (chatClient: IChatClient) (text: string) (cancellationToken: CancellationToken) =
    taskSeq {
        let prompt = createWordLookupPrompt text

        let stream =
            chatClient.CompleteChatStreamingAsync prompt (float32 0.1) 4096 cancellationToken

        let processedStream =
            processStream JsonDelimiter stream cancellationToken

        for event in processedStream do
            match event with
            | StreamEvent.TextChunk text -> yield SseItem<string>(text, eventType = "text")
            | StreamEvent.ResultChunk json -> yield SseItem<string>(json, eventType = "result")
    }

let mapDictionaryEndpoints(group: RouteGroupBuilder) =
    group
        .MapGet(
            Urls.Lookup,
            Func<string, IChatClient, CancellationToken, IResult>(fun text chatClient cancellationToken ->
                if String.IsNullOrWhiteSpace(text) then
                    Results.BadRequest({| error = "Text parameter is required" |})
                else
                    TypedResults.ServerSentEvents(streamLookup chatClient text cancellationToken))
        )
        .Produces<string>(StatusCodes.Status200OK, "text/event-stream")
        .Produces(StatusCodes.Status400BadRequest)
    |> ignore
