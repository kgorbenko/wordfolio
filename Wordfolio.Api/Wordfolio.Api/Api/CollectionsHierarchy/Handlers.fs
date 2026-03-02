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

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.CollectionsHierarchy

let mapCollectionsHierarchyEndpoints(group: IEndpointRouteBuilder) =
    group
        .MapGet(
            UrlTokens.Root,
            Func<ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result = getByUserId env { UserId = UserId userId }

                        let hierarchyResult =
                            failOnUnitError "CollectionsHierarchy.getByUserId" result

                        let response: CollectionsHierarchyResponse =
                            { Collections =
                                hierarchyResult.Collections
                                |> List.map toCollectionWithVocabulariesHierarchyResponse
                              DefaultVocabulary =
                                hierarchyResult.DefaultVocabulary
                                |> Option.map toVocabularyWithEntryCountHierarchyResponse }

                        return Results.Ok(response)
                })
        )
        .Produces<CollectionsHierarchyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore

    group
        .MapGet(
            Urls.CollectionsPath,
            Func<string, CollectionSortByRequest, SortDirectionRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun search sortBy sortDirection user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let query =
                                toSearchQuery search sortBy sortDirection

                            let! result =
                                searchUserCollections
                                    env
                                    { UserId = UserId userId
                                      Query = query }

                            let collections =
                                failOnUnitError "CollectionsHierarchy.searchUserCollections" result

                            return
                                collections
                                |> List.map toCollectionOverviewResponse
                                |> Results.Ok
                    })
        )
        .Produces<CollectionOverviewResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore

    group
        .MapGet(
            Urls.VocabulariesByCollectionPath,
            Func<int, string, VocabularySummarySortByRequest, SortDirectionRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId search sortBy sortDirection user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let query =
                                toCollectionVocabulariesQuery search sortBy sortDirection

                            let! result =
                                searchCollectionVocabularies
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId
                                      Query = query }

                            let vocabularies =
                                failOnUnitError "CollectionsHierarchy.searchCollectionVocabularies" result

                            return
                                vocabularies
                                |> List.map toVocabularyWithEntryCountHierarchyResponse
                                |> Results.Ok
                    })
        )
        .Produces<VocabularyWithEntryCountHierarchyResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore
