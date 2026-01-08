module Wordfolio.Api.Infrastructure.GroqChatClient

open System
open System.ClientModel
open System.Collections.Generic
open System.Threading

open FSharp.Control

open Microsoft.Extensions.Options

open OpenAI
open OpenAI.Chat

open Wordfolio.Api.Configuration.GroqApi
open Wordfolio.Api.Infrastructure.ChatClient

type GroqChatClient(config: IOptions<GroqApiConfiguration>) =

    let options =
        OpenAIClientOptions(Endpoint = Uri(config.Value.Url))

    let client =
        OpenAIClient(ApiKeyCredential(config.Value.ApiKey), options)

    let chatClient =
        client.GetChatClient(config.Value.Model)

    interface IChatClient with
        member _.CompleteChatStreamingAsync
            (prompt: string)
            (temperature: float32)
            (maxOutputTokenCount: int)
            (cancellationToken: CancellationToken)
            : IAsyncEnumerable<string> =
            taskSeq {
                let chatOptions =
                    ChatCompletionOptions(Temperature = temperature, MaxOutputTokenCount = maxOutputTokenCount)

                let messages =
                    [| ChatMessage.CreateUserMessage(prompt) :> ChatMessage |]

                let stream =
                    chatClient.CompleteChatStreamingAsync(messages, chatOptions, cancellationToken)

                for update in stream do
                    for part in update.ContentUpdate do
                        yield part.Text
            }
