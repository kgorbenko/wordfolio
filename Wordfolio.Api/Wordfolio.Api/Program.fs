open System

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options

open Wordfolio.Api.Identity
open Wordfolio.Api.IdentityIntegration
open Wordfolio.ServiceDefaults.Builder
open Wordfolio.ServiceDefaults.HealthCheck
open Wordfolio.ServiceDefaults.OpenApi
open Wordfolio.ServiceDefaults.Status

type Program() =
    class
    end

type WeatherForecast =
    { Date: DateOnly
      TemperatureC: int
      Summary: string option }

[<CLIMutable>]
type PasswordRequirements =
    { RequiredLength: int
      RequireDigit: bool
      RequireLowercase: bool
      RequireUppercase: bool
      RequireNonAlphanumeric: bool
      RequiredUniqueChars: int }

[<EntryPoint>]
let main args =
    let builder =
        WebApplication.CreateBuilder(args)
        |> addIdentity
        |> addAuthentication
        |> configureOpenTelemetry
        |> addDefaultHealthChecks
        |> addServiceDiscovery
        |> configureHttpClientDefaults
        |> addOpenApi

    builder.AddNpgsqlDataSource("wordfoliodb")

    let app = builder.Build()

    app |> mapOpenApi |> ignore

    app.UseAuthentication() |> ignore
    app.UseAuthorization() |> ignore

    app
    |> mapHealthChecks
    |> mapStatusEndpoint
    |> _.MapGroup("auth").MapIdentityApi<User>().AllowAnonymous()
    |> ignore

    app
        .MapGet(
            "/auth/password-requirements",
            Func<IOptions<IdentityOptions>, PasswordRequirements>(fun identityOptions ->
                let passwordOptions =
                    identityOptions.Value.Password

                { RequiredLength = passwordOptions.RequiredLength
                  RequireDigit = passwordOptions.RequireDigit
                  RequireLowercase = passwordOptions.RequireLowercase
                  RequireUppercase = passwordOptions.RequireUppercase
                  RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric
                  RequiredUniqueChars = passwordOptions.RequiredUniqueChars })
        )
        .AllowAnonymous()
    |> ignore

    app.MapGet(
        "/weatherforecast",
        Func<WeatherForecast[]>(fun source ->
            [ 1..5 ]
            |> Seq.map(fun index ->
                { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index))
                  TemperatureC = Random.Shared.Next(-20, 55)
                  Summary = Some "Hot" })
            |> Array.ofSeq)
    )
    |> ignore

    app.Run()

    0
