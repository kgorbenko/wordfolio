open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing

open Microsoft.Extensions.Hosting

open Wordfolio.Api
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

let mapEndpoints(app: IEndpointRouteBuilder) =
    let collectionsGroup =
        app.MapGroup(Urls.Collections.Path)

    mapCollectionsEndpoints collectionsGroup

    collectionsGroup.MapGroup(Urls.Vocabularies.Path)
    |> mapVocabulariesEndpoints

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
    |> mapEndpoints

    app.Run()

    0
