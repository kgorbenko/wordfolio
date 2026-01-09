module Wordfolio.Api.Tests.Utils.FakeChatClient

open System.Collections.Generic
open System.Threading

open FSharp.Control

open Wordfolio.Api.Infrastructure.ChatClient

type FakeChatClient(responseChunks: string list) =
    interface IChatClient with
        member _.CompleteChatStreamingAsync
            (prompt: string)
            (temperature: float32)
            (maxOutputTokenCount: int)
            (cancellationToken: CancellationToken)
            : IAsyncEnumerable<string> =
            taskSeq {
                for chunk in responseChunks do
                    cancellationToken.ThrowIfCancellationRequested()
                    yield chunk
            }
