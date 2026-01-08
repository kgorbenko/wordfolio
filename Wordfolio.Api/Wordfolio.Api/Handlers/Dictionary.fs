module Wordfolio.Api.Handlers.Dictionary

open System
open System.ClientModel
open System.Net.ServerSentEvents
open System.Runtime.CompilerServices
open System.Text
open System.Threading

open FSharp.Control

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Logging

open OpenAI
open OpenAI.Chat

open Wordfolio.Api.Configuration.GroqApi

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

let private streamLookup
    (groqConfig: GroqApiConfiguration)
    (text: string)
    ([<EnumeratorCancellation>] cancellationToken: CancellationToken)
    =
    taskSeq {
        let prompt = createWordLookupPrompt text

        let options =
            OpenAIClientOptions(Endpoint = Uri(groqConfig.Url))

        let client =
            OpenAIClient(ApiKeyCredential(groqConfig.ApiKey), options)

        let chatClient =
            client.GetChatClient(groqConfig.Model)

        let chatOptions =
            ChatCompletionOptions(Temperature = float32 0.1, MaxOutputTokenCount = 4096)

        let messages =
            [| ChatMessage.CreateUserMessage(prompt) :> ChatMessage |]

        let buffer = StringBuilder()
        let mutable inJsonPhase = false

        let stream =
            chatClient.CompleteChatStreamingAsync(messages, chatOptions, cancellationToken)

        for update in stream do
            for part in update.ContentUpdate do
                let content = part.Text

                if not(String.IsNullOrEmpty content) then
                    buffer.Append(content) |> ignore

                    if not inJsonPhase then
                        let bufferText = buffer.ToString()

                        match bufferText.IndexOf(JsonDelimiter) with
                        | -1 ->
                            let safeEnd =
                                bufferText.Length - JsonDelimiter.Length

                            if safeEnd > 0 then
                                yield SseItem<string>(bufferText.Substring(0, safeEnd), eventType = "text")

                                buffer.Clear().Append(bufferText.Substring(safeEnd))
                                |> ignore
                        | idx ->
                            inJsonPhase <- true

                            if idx > 0 then
                                yield SseItem<string>(bufferText.Substring(0, idx), eventType = "text")

                            buffer.Clear().Append(bufferText.Substring(idx + JsonDelimiter.Length))
                            |> ignore

        let remaining = buffer.ToString().Trim()

        if remaining.Length > 0 then
            let eventType =
                if inJsonPhase then "result" else "text"

            yield SseItem<string>(remaining, eventType = eventType)
    }

let mapDictionaryEndpoints(group: RouteGroupBuilder) =
    group.MapGet(
        Urls.Lookup,
        Func<string, GroqApiConfiguration, ILoggerFactory, CancellationToken, IResult>
            (fun text groqConfig loggerFactory cancellationToken ->
                if String.IsNullOrWhiteSpace(text) then
                    Results.BadRequest({| error = "Text parameter is required" |})
                else
                    TypedResults.ServerSentEvents(streamLookup groqConfig text cancellationToken))
    )
    |> ignore
