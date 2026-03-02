module Wordfolio.Api.Api.Collections.Handlers

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api
open Wordfolio.Api.Api.Collections.Mappers
open Wordfolio.Api.Api.Collections
open Wordfolio.Api.Api.Helpers
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations
open Wordfolio.Api.Infrastructure.Environment

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Collections

let private toGetByIdErrorResponse(error: GetCollectionByIdError) : IResult =
    match error with
    | GetCollectionByIdError.CollectionNotFound _ -> Results.NotFound()
    | GetCollectionByIdError.CollectionAccessDenied _ -> Results.Forbid()

let private toCreateErrorResponse(error: CreateCollectionError) : IResult =
    match error with
    | CreateCollectionError.CollectionNameRequired -> Results.BadRequest({| error = "Name is required" |})
    | CreateCollectionError.CollectionNameTooLong maxLength ->
        Results.BadRequest({| error = $"Name must be at most {maxLength} characters" |})

let private toUpdateErrorResponse(error: UpdateCollectionError) : IResult =
    match error with
    | UpdateCollectionError.CollectionNotFound _ -> Results.NotFound()
    | UpdateCollectionError.CollectionAccessDenied _ -> Results.Forbid()
    | UpdateCollectionError.CollectionNameRequired -> Results.BadRequest({| error = "Name is required" |})
    | UpdateCollectionError.CollectionNameTooLong maxLength ->
        Results.BadRequest({| error = $"Name must be at most {maxLength} characters" |})

let private toDeleteErrorResponse(error: DeleteCollectionError) : IResult =
    match error with
    | DeleteCollectionError.CollectionNotFound _ -> Results.NotFound()
    | DeleteCollectionError.CollectionAccessDenied _ -> Results.Forbid()

let mapCollectionsEndpoints(group: RouteGroupBuilder) =
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

                        let collections =
                            failOnUnitError "Collections.getByUserId" result

                        return
                            Results.Ok(
                                collections
                                |> List.map toCollectionResponse
                            )
                })
        )
        .Produces<CollectionResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore

    group
        .MapGet(
            UrlTokens.ById,
            Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun id user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                getById
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId id }

                            return
                                match result with
                                | Ok collection -> Results.Ok(toCollectionResponse collection)
                                | Error error -> toGetByIdErrorResponse error
                    })
        )
        .Produces<CollectionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPost(
            UrlTokens.Root,
            Func<CreateCollectionRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun request user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                create
                                    env
                                    { UserId = UserId userId
                                      Name = request.Name
                                      Description = request.Description
                                      CreatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok collection ->
                                    Results.Created(
                                        Urls.collectionById(CollectionId.value collection.Id),
                                        toCollectionResponse collection
                                    )
                                | Error error -> toCreateErrorResponse error
                    })
        )
        .Produces<CollectionResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
    |> ignore

    group
        .MapPut(
            UrlTokens.ById,
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
                                    { UserId = UserId userId
                                      CollectionId = CollectionId id
                                      Name = request.Name
                                      Description = request.Description
                                      UpdatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok collection -> Results.Ok(toCollectionResponse collection)
                                | Error error -> toUpdateErrorResponse error
                    })
        )
        .Produces<CollectionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapDelete(
            UrlTokens.ById,
            Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun id user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                delete
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId id }

                            return
                                match result with
                                | Ok() -> Results.NoContent()
                                | Error error -> toDeleteErrorResponse error
                    })
        )
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore
