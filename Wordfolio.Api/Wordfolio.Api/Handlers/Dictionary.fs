module Wordfolio.Api.Handlers.Dictionary

open System

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Dictionary

type ExampleResponse = { Text: string }

type DefinitionResponse =
    { DefinitionText: string
      Examples: ExampleResponse list }

type TranslationResponse =
    { TranslationText: string
      Examples: ExampleResponse list }

type LookupResponse =
    { Text: string
      Definitions: DefinitionResponse list
      Translations: TranslationResponse list }

let private getStubData(text: string) : LookupResponse =
    { Text = text
      Definitions =
        [ { DefinitionText = "the state of being free from tension and anxiety"
            Examples = [ { Text = "You should try to relax after a long day of work." } ] }
          { DefinitionText = "to make or become less tense or anxious"
            Examples = [ { Text = "Take a deep breath and relax your shoulders." } ] }
          { DefinitionText = "to rest or engage in an enjoyable activity so as to become less tired or anxious"
            Examples = [ { Text = "We spent the weekend relaxing at the beach." } ] } ]
      Translations =
        [ { TranslationText = "расслабляться"
            Examples = [ { Text = "Тебе нужно расслабиться после долгого рабочего дня." } ] }
          { TranslationText = "отдыхать"
            Examples = [ { Text = "Мы провели выходные, отдыхая на пляже." } ] } ] }

let mapDictionaryEndpoints(group: RouteGroupBuilder) =
    group.MapGet(
        Urls.Lookup,
        Func<string, _>(fun text ->
            task {
                if String.IsNullOrWhiteSpace(text) then
                    return Results.BadRequest({| error = "Text parameter is required" |})
                else
                    let response = getStubData text
                    return Results.Ok(response)
            })
    )
    |> ignore
