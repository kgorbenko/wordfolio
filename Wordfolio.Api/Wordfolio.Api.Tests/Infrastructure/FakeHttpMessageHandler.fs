module Wordfolio.Api.Tests.Infrastructure.FakeHttpMessageHandler

open System.Net.Http
open System.Threading
open System.Threading.Tasks

type FakeHttpMessageHandler(responseFunc: HttpRequestMessage -> HttpResponseMessage) =
    inherit HttpMessageHandler()

    let requestHistory =
        ResizeArray<HttpRequestMessage>()

    member _.RequestHistory =
        requestHistory |> Seq.toList

    override _.SendAsync(request: HttpRequestMessage, _: CancellationToken) =
        requestHistory.Add(request)
        Task.FromResult(responseFunc request)
