module Wordfolio.Api.Api.CollectionsHierarchy.Handlers

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api
open Wordfolio.Api.Api.CollectionsHierarchy.Mappers
open Wordfolio.Api.Api.CollectionsHierarchy.Types
open Wordfolio.Api.Api.Helpers
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations
open Wordfolio.Api.Infrastructure.Environment
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.CollectionsHierarchy

let mapCollectionsHierarchyEndpoints(group: IEndpointRouteBuilder) =
    group
        .MapGet(
            UrlTokens.Root,
            Func<ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result = getByUserId env { UserId = UserId userId }

                            let hierarchyResult = okOrFail result

                            let response: CollectionsHierarchyResultResponse =
                                { Collections =
                                    hierarchyResult.Collections
                                    |> List.map(toCollectionWithVocabulariesResponse encoder)
                                  DefaultVocabulary =
                                    hierarchyResult.DefaultVocabulary
                                    |> Option.map(toVocabularyWithEntryCountResponse encoder) }

                            return Results.Ok(response)
                    })
        )
        .Produces<CollectionsHierarchyResultResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore

    group
        .MapGet(
            Urls.CollectionsPath,
            Func<ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result = getCollectionsWithVocabularyCount env (UserId userId)

                            let collections = okOrFail result

                            return
                                collections
                                |> List.map(toCollectionWithVocabularyCountResponse encoder)
                                |> Results.Ok
                    })
        )
        .Produces<CollectionWithVocabularyCountResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore

    group
        .MapGet(
            Urls.VocabulariesByCollectionPath,
            Func<string, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user, encoder.Decode(collectionId) with
                        | None, _ -> return Results.Unauthorized()
                        | _, None -> return Results.NotFound()
                        | Some userId, Some collectionId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                getVocabulariesWithEntryCountByCollectionId
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId }

                            let vocabularies = okOrFail result

                            return
                                vocabularies
                                |> List.map(toVocabularyWithEntryCountResponse encoder)
                                |> Results.Ok
                    })
        )
        .Produces<VocabularyWithEntryCountResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore
