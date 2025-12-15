module Wordfolio.Api.Handlers.Vocabularies

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations
open Wordfolio.Api.Infrastructure.Environment

module Urls =
    [<Literal>]
    let VocabulariesByCollection =
        "/collections/{collectionId:int}/vocabularies"

    [<Literal>]
    let VocabularyById =
        "/vocabularies/{id:int}"

type VocabularyResponse =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateVocabularyRequest =
    { Name: string
      Description: string option }

type UpdateVocabularyRequest =
    { Name: string
      Description: string option }

let private toResponse(vocabulary: Vocabulary) : VocabularyResponse =
    { Id = VocabularyId.value vocabulary.Id
      CollectionId = CollectionId.value vocabulary.CollectionId
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt }

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

let private toErrorResponse(error: VocabularyError) : IResult =
    match error with
    | VocabularyNotFound _ -> Results.NotFound()
    | VocabularyAccessDenied _ -> Results.Forbid()
    | VocabularyNameRequired -> Results.BadRequest({| error = "Name is required" |})
    | VocabularyNameTooLong maxLength ->
        Results.BadRequest({| error = $"Name must be at most {maxLength} characters" |})
    | VocabularyCollectionNotFound _ -> Results.NotFound({| error = "Collection not found" |})

let mapVocabulariesEndpoints(app: IEndpointRouteBuilder) =
    app
        .MapGet(
            Urls.VocabulariesByCollection,
            Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let env =
                                NonTransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                Transactions.runInTransaction env (fun appEnv ->
                                    getByCollectionId appEnv (UserId userId) (CollectionId collectionId))

                            return
                                match result with
                                | Ok vocabularies ->
                                    let response =
                                        vocabularies |> List.map toResponse

                                    Results.Ok(response) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapGet(
            Urls.VocabularyById,
            Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun id user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let env =
                                NonTransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                Transactions.runInTransaction env (fun appEnv ->
                                    getById appEnv (UserId userId) (VocabularyId id))

                            return
                                match result with
                                | Ok vocabulary -> Results.Ok(toResponse vocabulary) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPost(
            Urls.VocabulariesByCollection,
            Func<int, CreateVocabularyRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId request user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                Transactions.runInTransaction env (fun appEnv ->
                                    create
                                        appEnv
                                        (UserId userId)
                                        (CollectionId collectionId)
                                        request.Name
                                        request.Description
                                        DateTimeOffset.UtcNow)

                            return
                                match result with
                                | Ok vocabulary ->
                                    Results.Created(
                                        $"/vocabularies/{VocabularyId.value vocabulary.Id}",
                                        toResponse vocabulary
                                    )
                                    :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPut(
            Urls.VocabularyById,
            Func<int, UpdateVocabularyRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun id request user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                Transactions.runInTransaction env (fun appEnv ->
                                    update
                                        appEnv
                                        (UserId userId)
                                        (VocabularyId id)
                                        request.Name
                                        request.Description
                                        DateTimeOffset.UtcNow)

                            return
                                match result with
                                | Ok vocabulary -> Results.Ok(toResponse vocabulary) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapDelete(
            Urls.VocabularyById,
            Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun id user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                Transactions.runInTransaction env (fun appEnv ->
                                    delete appEnv (UserId userId) (VocabularyId id))

                            return
                                match result with
                                | Ok() -> Results.NoContent() :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
