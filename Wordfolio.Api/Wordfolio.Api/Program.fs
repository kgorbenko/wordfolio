open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Wordfolio.Api
open Wordfolio.Api.Configuration.GroqApi
open Wordfolio.Api.DataAccess
open Wordfolio.Api.Api.Auth.Handlers
open Wordfolio.Api.Api.Collections.Handlers
open Wordfolio.Api.Api.CollectionsHierarchy.Handlers
open Wordfolio.Api.Api.Dictionary.Handlers
open Wordfolio.Api.Api.Drafts.Handlers
open Wordfolio.Api.Api.Entries.Handlers
open Wordfolio.Api.Api.Vocabularies.Handlers
open Wordfolio.Api.IdentityIntegration
open Wordfolio.Api.Infrastructure.ChatClient
open Wordfolio.Api.Infrastructure.GroqChatClient
open Wordfolio.ServiceDefaults.Builder
open Wordfolio.ServiceDefaults.HealthCheck
open Wordfolio.ServiceDefaults.OpenApi
open Wordfolio.ServiceDefaults.Status

open Wordfolio.Api.OpenApi

type Program() =
    class
    end

let mapEndpoints(app: IEndpointRouteBuilder) =
    let collectionsGroup =
        app.MapGroup(Urls.Collections.Path).WithTags("Collections")

    mapCollectionsEndpoints collectionsGroup

    app.MapGroup(Urls.CollectionsHierarchy.Path).WithTags("CollectionsHierarchy")
    |> mapCollectionsHierarchyEndpoints

    let vocabulariesGroup =
        collectionsGroup.MapGroup(Urls.Vocabularies.Path).WithTags("Vocabularies")

    mapVocabulariesEndpoints vocabulariesGroup

    vocabulariesGroup.MapGroup(Urls.Entries.Path).WithTags("Entries")
    |> mapEntriesEndpoints

    app.MapGroup(Urls.Drafts.Path).WithTags("Drafts")
    |> mapDraftsEndpoints

    app.MapGroup(Urls.Auth.Path).WithTags("Auth")
    |> mapAuthEndpoints

    app.MapGroup(Urls.Dictionary.Path).WithTags("Dictionary")
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

    builder.Services.AddOptions<GroqApiConfiguration>().BindConfiguration("GroqApi")
    |> ignore

    builder.Services.AddSingleton<IChatClient, GroqChatClient>()
    |> ignore

    Dapper.registerTypes()

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
