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
    let group = app.MapGroup("/collections")

    group.MapGet(
        "/",
        Func<ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun user dataSource cancellationToken ->
            task {
                match getUserId user with
                | None -> return Results.Unauthorized()
                | Some userId ->
                    let env =
                        TransactionalEnv(dataSource, cancellationToken)

                    let! result = getByUserId env (UserId userId)
                    let response = result |> List.map toResponse
                    return Results.Ok(response)
            })
    )
    |> ignore

    group.MapGet(
        "/{id:int}",
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun id user dataSource cancellationToken ->
            task {
                match getUserId user with
                | None -> return Results.Unauthorized()
                | Some userId ->
                    let env =
                        TransactionalEnv(dataSource, cancellationToken)

                    let! result = getById env (UserId userId) (CollectionId id)

                    return
                        match result with
                        | Ok collection -> Results.Ok(toResponse collection)
                        | Error error -> toErrorResponse error
            })
    )
    |> ignore

    group.MapPost(
        "/",
        Func<CreateCollectionRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun request user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result = create env (UserId userId) request.Name request.Description DateTimeOffset.UtcNow

                        return
                            match result with
                            | Ok collection ->
                                Results.Created(
                                    $"/collections/{CollectionId.value collection.Id}",
                                    toResponse collection
                                )
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    group.MapPut(
        "/{id:int}",
        Func<int, UpdateCollectionRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun id request user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result =
                            update
                                env
                                (UserId userId)
                                (CollectionId id)
                                request.Name
                                request.Description
                                DateTimeOffset.UtcNow

                        return
                            match result with
                            | Ok collection -> Results.Ok(toResponse collection)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    group.MapDelete(
        "/{id:int}",
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun id user dataSource cancellationToken ->
            task {
                match getUserId user with
                | None -> return Results.Unauthorized()
                | Some userId ->
                    let env =
                        TransactionalEnv(dataSource, cancellationToken)

                    let! result = delete env (UserId userId) (CollectionId id)

                    return
                        match result with
                        | Ok() -> Results.NoContent()
                        | Error error -> toErrorResponse error
            })
    )
    |> ignore

    app
