open System

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting

open Wordfolio.ServiceDefaults.Builder
open Wordfolio.ServiceDefaults.HealthCheck
open Wordfolio.ServiceDefaults.OpenApi
open Wordfolio.ServiceDefaults.Status

type WeatherForecast =
    { Date: DateOnly
      TemperatureC: int
      Summary: string option }

[<EntryPoint>]
let main args =
    let builder =
        WebApplication.CreateBuilder(args)
        |> configureOpenTelemetry
        |> addDefaultHealthChecks
        |> addServiceDiscovery
        |> configureHttpClientDefaults
        |> addOpenApi

    let app = builder.Build()

    app
    |> mapOpenApi
    |> mapHealthChecks
    |> mapStatusEndpoint
    |> ignore

    app.MapGet("/weatherforecast", Func<WeatherForecast[]>(fun () ->
        [1..5]
        |> Seq.map (fun index ->
            { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index))
              TemperatureC = Random.Shared.Next(-20, 55)
              Summary = Some "Hot" }
        )
        |> Array.ofSeq
    )) |> ignore

    app.Run()

    0