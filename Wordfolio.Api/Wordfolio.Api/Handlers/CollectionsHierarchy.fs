module Wordfolio.Api.Handlers.CollectionsHierarchy

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations
open Wordfolio.Api.Infrastructure.Environment

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.CollectionsHierarchy

type CollectionSortByRequest =
    | Name = 0
    | CreatedAt = 1
    | UpdatedAt = 2
    | VocabularyCount = 3

type VocabularySummarySortByRequest =
    | Name = 0
    | CreatedAt = 1
    | UpdatedAt = 2
    | EntryCount = 3

type SortDirectionRequest =
    | Asc = 0
    | Desc = 1

type VocabularyWithEntryCountHierarchyResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int }

type CollectionWithVocabulariesHierarchyResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularyWithEntryCountHierarchyResponse list }

type CollectionOverviewResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      VocabularyCount: int }

type VocabularySummaryResponse = VocabularyWithEntryCountHierarchyResponse
type CollectionSummaryResponse = CollectionOverviewResponse

type CollectionsHierarchyResponse =
    { Collections: CollectionWithVocabulariesHierarchyResponse list
      DefaultVocabulary: VocabularyWithEntryCountHierarchyResponse option }

let private toVocabularyWithEntryCountHierarchyResponse
    (v: VocabularySummary)
    : VocabularyWithEntryCountHierarchyResponse =
    { Id = VocabularyId.value v.Id
      Name = v.Name
      Description = v.Description
      CreatedAt = v.CreatedAt
      UpdatedAt = v.UpdatedAt
      EntryCount = v.EntryCount }

let private toCollectionWithVocabulariesHierarchyResponse
    (c: CollectionSummary)
    : CollectionWithVocabulariesHierarchyResponse =
    { Id = CollectionId.value c.Id
      Name = c.Name
      Description = c.Description
      CreatedAt = c.CreatedAt
      UpdatedAt = c.UpdatedAt
      Vocabularies =
        c.Vocabularies
        |> List.map toVocabularyWithEntryCountHierarchyResponse }

let private toCollectionOverviewResponse(c: CollectionOverview) : CollectionOverviewResponse =
    { Id = CollectionId.value c.Id
      Name = c.Name
      Description = c.Description
      CreatedAt = c.CreatedAt
      UpdatedAt = c.UpdatedAt
      VocabularyCount = c.VocabularyCount }

let private toVocabularySortByDomain(sortBy: VocabularySummarySortByRequest) =
    match sortBy with
    | VocabularySummarySortByRequest.Name -> VocabularySummarySortBy.Name
    | VocabularySummarySortByRequest.CreatedAt -> VocabularySummarySortBy.CreatedAt
    | VocabularySummarySortByRequest.UpdatedAt -> VocabularySummarySortBy.UpdatedAt
    | VocabularySummarySortByRequest.EntryCount -> VocabularySummarySortBy.EntryCount
    | _ -> VocabularySummarySortBy.Name

let private toSortByDomain(sortBy: CollectionSortByRequest) =
    match sortBy with
    | CollectionSortByRequest.Name -> CollectionSortBy.Name
    | CollectionSortByRequest.CreatedAt -> CollectionSortBy.CreatedAt
    | CollectionSortByRequest.UpdatedAt -> CollectionSortBy.UpdatedAt
    | CollectionSortByRequest.VocabularyCount -> CollectionSortBy.VocabularyCount
    | _ -> CollectionSortBy.Name

let private toSortDirectionDomain(sortDirection: SortDirectionRequest) =
    match sortDirection with
    | SortDirectionRequest.Asc -> SortDirection.Asc
    | SortDirectionRequest.Desc -> SortDirection.Desc
    | _ -> SortDirection.Asc

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

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

                        let! result = getByUserId env (UserId userId)

                        return
                            match result with
                            | Ok hierarchyResult ->
                                let response: CollectionsHierarchyResponse =
                                    { Collections =
                                        hierarchyResult.Collections
                                        |> List.map toCollectionWithVocabulariesHierarchyResponse
                                      DefaultVocabulary =
                                        hierarchyResult.DefaultVocabulary
                                        |> Option.map toVocabularyWithEntryCountHierarchyResponse }

                                Results.Ok(response)
                            | Error _ ->
                                Results.Ok(
                                    { Collections = []
                                      DefaultVocabulary = None }
                                )
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

                            let query: SearchUserCollectionsQuery =
                                { Search =
                                    if String.IsNullOrWhiteSpace search then
                                        None
                                    else
                                        Some search
                                  SortBy = sortBy |> toSortByDomain
                                  SortDirection = sortDirection |> toSortDirectionDomain }

                            let! result = searchUserCollections env (UserId userId) query

                            return
                                match result with
                                | Ok collections ->
                                    collections
                                    |> List.map toCollectionOverviewResponse
                                    |> Results.Ok
                                | Error _ -> Results.Ok([])
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

                            let query: VocabularySummaryQuery =
                                { Search =
                                    if String.IsNullOrWhiteSpace search then
                                        None
                                    else
                                        Some search
                                  SortBy = sortBy |> toVocabularySortByDomain
                                  SortDirection = sortDirection |> toSortDirectionDomain }

                            let! result =
                                searchCollectionVocabularies env (UserId userId) (CollectionId collectionId) query

                            return
                                match result with
                                | Ok vocabularies ->
                                    vocabularies
                                    |> List.map toVocabularyWithEntryCountHierarchyResponse
                                    |> Results.Ok
                                | Error _ -> Results.Ok([])
                    })
        )
        .Produces<VocabularyWithEntryCountHierarchyResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore
