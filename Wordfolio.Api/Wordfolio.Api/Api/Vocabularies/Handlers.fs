module Wordfolio.Api.Api.Vocabularies.Handlers

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api
open Wordfolio.Api.Api.Helpers
open Wordfolio.Api.Api.Vocabularies.Mappers
open Wordfolio.Api.Api.Vocabularies.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations
open Wordfolio.Api.Infrastructure.Environment

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Vocabularies

let private toGetByIdErrorResponse(error: GetVocabularyByIdError) : IResult =
    match error with
    | GetVocabularyByIdError.VocabularyNotFound _ -> Results.NotFound()
    | GetVocabularyByIdError.VocabularyAccessDenied _ -> Results.Forbid()
    | GetVocabularyByIdError.VocabularyCollectionNotFound _ -> Results.NotFound({| error = "Collection not found" |})

let private toGetByCollectionIdErrorResponse(error: GetVocabulariesByCollectionIdError) : IResult =
    match error with
    | GetVocabulariesByCollectionIdError.VocabularyCollectionNotFound _ ->
        Results.NotFound({| error = "Collection not found" |})

let private toCreateErrorResponse(error: CreateVocabularyError) : IResult =
    match error with
    | CreateVocabularyError.VocabularyNameRequired -> Results.BadRequest({| error = "Name is required" |})
    | CreateVocabularyError.VocabularyNameTooLong maxLength ->
        Results.BadRequest({| error = $"Name must be at most {maxLength} characters" |})
    | CreateVocabularyError.VocabularyCollectionNotFound _ -> Results.NotFound({| error = "Collection not found" |})

let private toUpdateErrorResponse(error: UpdateVocabularyError) : IResult =
    match error with
    | UpdateVocabularyError.VocabularyNotFound _ -> Results.NotFound()
    | UpdateVocabularyError.VocabularyAccessDenied _ -> Results.Forbid()
    | UpdateVocabularyError.VocabularyNameRequired -> Results.BadRequest({| error = "Name is required" |})
    | UpdateVocabularyError.VocabularyNameTooLong maxLength ->
        Results.BadRequest({| error = $"Name must be at most {maxLength} characters" |})

let private toDeleteErrorResponse(error: DeleteVocabularyError) : IResult =
    match error with
    | DeleteVocabularyError.VocabularyNotFound _ -> Results.NotFound()
    | DeleteVocabularyError.VocabularyAccessDenied _ -> Results.Forbid()

let mapVocabulariesEndpoints(group: RouteGroupBuilder) =
    group
        .MapGet(
            UrlTokens.Root,
            Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                getByCollectionId
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId }

                            return
                                match result with
                                | Ok vocabularies ->
                                    let response =
                                        vocabularies
                                        |> List.map toVocabularyResponse

                                    Results.Ok(response)
                                | Error error -> toGetByCollectionIdErrorResponse error
                    })
        )
        .Produces<VocabularyResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPost(
            UrlTokens.Root,
            Func<int, CreateVocabularyRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId request user dataSource cancellationToken ->
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
                                      CollectionId = CollectionId collectionId
                                      Name = request.Name
                                      Description = request.Description
                                      CreatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok vocabulary ->
                                    Results.Created(
                                        Urls.vocabularyById(collectionId, VocabularyId.value vocabulary.Id),
                                        toVocabularyResponse vocabulary
                                    )
                                | Error error -> toCreateErrorResponse error
                    })
        )
        .Produces<VocabularyResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapGet(
            UrlTokens.ById,
            Func<int, int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId id user dataSource cancellationToken ->
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
                                      VocabularyId = VocabularyId id }

                            return
                                match result with
                                | Ok detail -> Results.Ok(toVocabularyDetailResponse detail)
                                | Error error -> toGetByIdErrorResponse error
                    })
        )
        .Produces<VocabularyDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPut(
            UrlTokens.ById,
            Func<int, int, UpdateVocabularyRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId id request user dataSource cancellationToken ->
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
                                      VocabularyId = VocabularyId id
                                      Name = request.Name
                                      Description = request.Description
                                      UpdatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok vocabulary -> Results.Ok(toVocabularyResponse vocabulary)
                                | Error error -> toUpdateErrorResponse error
                    })
        )
        .Produces<VocabularyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapDelete(
            UrlTokens.ById,
            Func<int, int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId id user dataSource cancellationToken ->
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
                                      VocabularyId = VocabularyId id }

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
