module Wordfolio.Api.Handlers.Dictionary

open System
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Wordfolio.Api.Infrastructure.WordsApi

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
