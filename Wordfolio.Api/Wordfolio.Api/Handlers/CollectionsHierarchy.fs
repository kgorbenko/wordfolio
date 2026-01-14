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

module Urls = Wordfolio.Api.Urls.CollectionsHierarchy

type VocabularySummaryResponse =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int }

type CollectionSummaryResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularySummaryResponse list }

let private toVocabularySummaryResponse(v: VocabularySummary) : VocabularySummaryResponse =
    { Id = VocabularyId.value v.Id
      CollectionId = CollectionId.value v.CollectionId
      Name = v.Name
      Description = v.Description
      CreatedAt = v.CreatedAt
      UpdatedAt = v.UpdatedAt
      EntryCount = v.EntryCount }

let private toCollectionSummaryResponse(c: CollectionSummary) : CollectionSummaryResponse =
    { Id = CollectionId.value c.Id
      Name = c.Name
      Description = c.Description
      CreatedAt = c.CreatedAt
      UpdatedAt = c.UpdatedAt
      Vocabularies =
        c.Vocabularies
        |> List.map toVocabularySummaryResponse }

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

let mapCollectionsHierarchyEndpoint(app: IEndpointRouteBuilder) =
    app
        .MapGet(
            Urls.Path,
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
                            | Ok collections ->
                                let response =
                                    collections
                                    |> List.map toCollectionSummaryResponse

                                Results.Ok(response)
                            | Error _ -> Results.Ok([])
                })
        )
        .WithTags("Collections")
        .Produces<CollectionSummaryResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore
