module Wordfolio.Api.Infrastructure.ChatClient

open System.Collections.Generic
open System.Threading

type IChatClient =
    abstract CompleteChatStreamingAsync:
        prompt: string ->
        temperature: float32 ->
        maxOutputTokenCount: int ->
        cancellationToken: CancellationToken ->
            IAsyncEnumerable<string>
