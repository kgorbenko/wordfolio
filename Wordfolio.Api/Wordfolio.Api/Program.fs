open Microsoft.AspNetCore.Builder

open Microsoft.Extensions.Hosting

open Wordfolio.Api.Handlers.Auth
open Wordfolio.Api.Handlers.Collections
open Wordfolio.Api.Handlers.Vocabularies
open Wordfolio.Api.IdentityIntegration
open Wordfolio.ServiceDefaults.Builder
open Wordfolio.ServiceDefaults.HealthCheck
open Wordfolio.ServiceDefaults.OpenApi
open Wordfolio.ServiceDefaults.Status

type Program() =
    class
    end

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
    |> mapAuthEndpoints
    |> fun app ->
        let collectionsGroup =
            mapCollectionsEndpoints app

        mapVocabulariesEndpoints collectionsGroup
    |> ignore

    app.Run()

    0
