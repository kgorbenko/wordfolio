module Wordfolio.Api.Handlers.Collections

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations
open Wordfolio.Api.Infrastructure.Environment

module Urls =
    [<Literal>]
    let Collections = "/collections"

    [<Literal>]
    let CollectionById = "/collections/{id:int}"

type CollectionResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateCollectionRequest =
    { Name: string
      Description: string option }

type UpdateCollectionRequest =
    { Name: string
      Description: string option }

let private toResponse(collection: Collection) : CollectionResponse =
    { Id = CollectionId.value collection.Id
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt }

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

let private toErrorResponse(error: CollectionError) : IResult =
    match error with
    | CollectionNotFound _ -> Results.NotFound()
    | CollectionAccessDenied _ -> Results.Forbid()
    | CollectionNameRequired -> Results.BadRequest({| error = "Name is required" |})
    | CollectionNameTooLong maxLength ->
        Results.BadRequest({| error = $"Name must be at most {maxLength} characters" |})

let mapCollectionsEndpoints(app: IEndpointRouteBuilder) =
    app
        .MapGet(
            Urls.Collections,
            Func<ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized() :> IResult
                    | Some userId ->
                        let env =
                            NonTransactionalEnv(dataSource, cancellationToken)

                        let! collections =
                            Transactions.runInTransaction env (fun appEnv ->
                                task {
                                    let! result = getByUserId appEnv (UserId userId)
                                    return Ok result
                                })

                        match collections with
                        | Ok result ->
                            let response = result |> List.map toResponse
                            return Results.Ok(response) :> IResult
                        | Error _ -> return Results.StatusCode(500) :> IResult
                })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapGet(
            Urls.CollectionById,
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
                                    getById appEnv (UserId userId) (CollectionId id))

                            return
                                match result with
                                | Ok collection -> Results.Ok(toResponse collection) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPost(
            Urls.Collections,
            Func<CreateCollectionRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun request user dataSource cancellationToken ->
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
                                        request.Name
                                        request.Description
                                        DateTimeOffset.UtcNow)

                            return
                                match result with
                                | Ok collection ->
                                    Results.Created(
                                        $"/collections/{CollectionId.value collection.Id}",
                                        toResponse collection
                                    )
                                    :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPut(
            Urls.CollectionById,
            Func<int, UpdateCollectionRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
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
                                        (CollectionId id)
                                        request.Name
                                        request.Description
                                        DateTimeOffset.UtcNow)

                            return
                                match result with
                                | Ok collection -> Results.Ok(toResponse collection) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapDelete(
            Urls.CollectionById,
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
                                    delete appEnv (UserId userId) (CollectionId id))

                            return
                                match result with
                                | Ok() -> Results.NoContent() :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
