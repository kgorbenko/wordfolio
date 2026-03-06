module Wordfolio.Api.Api.Dictionary.Handlers

open System
open System.Net.ServerSentEvents
open System.Threading

open FSharp.Control

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Wordfolio.Api.Api.Dictionary.Mappers
open Wordfolio.Api.Api.Dictionary
open Wordfolio.Api.Infrastructure.ChatClient
open Wordfolio.Api.Infrastructure.DelimitedStreamProcessor

module Urls = Wordfolio.Api.Urls.Dictionary

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
                    let response: LookupErrorResponse =
                        { error = "Text parameter is required" }

                    Results.BadRequest(response)
                else
                    TypedResults.ServerSentEvents(streamLookup chatClient text cancellationToken))
        )
        .Produces<string>(StatusCodes.Status200OK, "text/event-stream")
        .Produces(StatusCodes.Status400BadRequest)
    |> ignore
