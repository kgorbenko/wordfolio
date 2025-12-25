open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Microsoft.Extensions.Hosting

open Wordfolio.Api
open Wordfolio.Api.Handlers.Auth
open Wordfolio.Api.Handlers.Collections
open Wordfolio.Api.Handlers.Dictionary
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
        app.MapGroup(Urls.Collections.Path).WithTags("Collections")

    mapCollectionsEndpoints collectionsGroup

    collectionsGroup.MapGroup(Urls.Vocabularies.Path).WithTags("Vocabularies")
    |> mapVocabulariesEndpoints

    app.MapGroup(Urls.Auth.Path).WithTags("Auth")
    |> mapAuthEndpoints

    app.MapGroup(Urls.Dictionary.Path)
    |> mapDictionaryEndpoints

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
    |> mapEndpoints

    app.Run()

    0
