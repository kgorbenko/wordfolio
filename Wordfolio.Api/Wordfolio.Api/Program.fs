open System

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Wordfolio.Api.Domain
open Wordfolio.Api.Handlers.Auth
open Wordfolio.Api.Handlers.Collections
open Wordfolio.Api.Handlers.Vocabularies
open Wordfolio.Api.IdentityIntegration
open Wordfolio.Api.Infrastructure.Repositories
open Wordfolio.ServiceDefaults.Builder
open Wordfolio.ServiceDefaults.HealthCheck
open Wordfolio.ServiceDefaults.OpenApi
open Wordfolio.ServiceDefaults.Status

type Program() =
    class
    end

let addRepositories<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    builder.Services.AddScoped<ICollectionRepository, CollectionRepository>()
    |> ignore

    builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>()
    |> ignore

    builder

[<EntryPoint>]
let main args =
    let builder =
        WebApplication.CreateBuilder(args)
        |> addIdentity
        |> addAuthentication
        |> addRepositories
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
    |> mapCollectionsEndpoints
    |> mapVocabulariesEndpoints
    |> ignore

    app.Run()

    0
