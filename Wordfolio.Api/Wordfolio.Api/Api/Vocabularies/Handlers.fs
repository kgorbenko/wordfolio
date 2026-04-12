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
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

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

let private toMoveErrorResponse(error: MoveVocabularyError) : IResult =
    match error with
    | MoveVocabularyError.VocabularyNotFound _ -> Results.NotFound()
    | MoveVocabularyError.VocabularyAccessDenied _ -> Results.NotFound()
    | MoveVocabularyError.CollectionNotFoundOrAccessDenied _ -> Results.NotFound()

let mapVocabulariesEndpoints(group: RouteGroupBuilder) =
    group
        .MapGet(
            UrlTokens.Root,
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
                                getByCollectionId
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId }

                            return
                                match result with
                                | Ok vocabularies ->
                                    let response =
                                        vocabularies
                                        |> List.map(toVocabularyResponse encoder)

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
            Func<string, CreateVocabularyRequest, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId request user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user, encoder.Decode(collectionId) with
                        | None, _ -> return Results.Unauthorized()
                        | _, None -> return Results.NotFound()
                        | Some userId, Some collectionId ->
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
                                    let response =
                                        toVocabularyResponse encoder vocabulary

                                    Results.Created(Urls.vocabularyById(response.CollectionId, response.Id), response)
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
            Func<string, string, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId id user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user, encoder.Decode(collectionId), encoder.Decode(id) with
                        | None, _, _ -> return Results.Unauthorized()
                        | _, None, _
                        | _, _, None -> return Results.NotFound()
                        | Some userId, Some collectionId, Some id ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                getById
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId id }

                            return
                                match result with
                                | Ok detail -> Results.Ok(toVocabularyDetailResponse encoder detail)
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
            Func<string, string, UpdateVocabularyRequest, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId id request user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user, encoder.Decode(collectionId), encoder.Decode(id) with
                        | None, _, _ -> return Results.Unauthorized()
                        | _, None, _
                        | _, _, None -> return Results.NotFound()
                        | Some userId, Some collectionId, Some id ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                update
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId id
                                      Name = request.Name
                                      Description = request.Description
                                      UpdatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok vocabulary -> Results.Ok(toVocabularyResponse encoder vocabulary)
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
            Func<string, string, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId id user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user, encoder.Decode(collectionId), encoder.Decode(id) with
                        | None, _, _ -> return Results.Unauthorized()
                        | _, None, _
                        | _, _, None -> return Results.NotFound()
                        | Some userId, Some collectionId, Some id ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                delete
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId
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

    group
        .MapPost(
            UrlTokens.ById + "/move",
            Func<string, string, MoveVocabularyRequest, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId id request user encoder dataSource cancellationToken ->
                    task {
                        match
                            getUserId user,
                            encoder.Decode(collectionId),
                            encoder.Decode(id),
                            encoder.Decode(request.CollectionId)
                        with
                        | None, _, _, _ -> return Results.Unauthorized()
                        | _, None, _, _
                        | _, _, None, _
                        | _, _, _, None -> return Results.NotFound()
                        | Some userId, Some collectionId, Some id, Some targetCollectionId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                move
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId id
                                      TargetCollectionId = CollectionId targetCollectionId
                                      UpdatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok vocabulary -> Results.Ok(toVocabularyResponse encoder vocabulary)
                                | Error error -> toMoveErrorResponse error
                    })
        )
        .Produces<VocabularyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore
