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

let mapVocabulariesEndpoints(collectionsGroup: IEndpointRouteBuilder) =
    let vocabulariesGroup =
        collectionsGroup.MapGroup("/{collectionId:int}/vocabularies")

    vocabulariesGroup.MapGet(
        "/",
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun collectionId user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result = getByCollectionId env (UserId userId) (CollectionId collectionId)

                        return
                            match result with
                            | Ok vocabularies ->
                                let response =
                                    vocabularies |> List.map toResponse

                                Results.Ok(response)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    vocabulariesGroup.MapPost(
        "/",
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
                                (UserId userId)
                                (CollectionId collectionId)
                                request.Name
                                request.Description
                                DateTimeOffset.UtcNow

                        return
                            match result with
                            | Ok vocabulary ->
                                Results.Created(
                                    $"/collections/{collectionId}/vocabularies/{VocabularyId.value vocabulary.Id}",
                                    toResponse vocabulary
                                )
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    vocabulariesGroup.MapGet(
        "/{id:int}",
        Func<int, int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun collectionId id user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result = getById env (UserId userId) (VocabularyId id)

                        return
                            match result with
                            | Ok vocabulary -> Results.Ok(toResponse vocabulary)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    vocabulariesGroup.MapPut(
        "/{id:int}",
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
                                (UserId userId)
                                (VocabularyId id)
                                request.Name
                                request.Description
                                DateTimeOffset.UtcNow

                        return
                            match result with
                            | Ok vocabulary -> Results.Ok(toResponse vocabulary)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    vocabulariesGroup.MapDelete(
        "/{id:int}",
        Func<int, int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun collectionId id user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result = delete env (UserId userId) (VocabularyId id)

                        return
                            match result with
                            | Ok() -> Results.NoContent()
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    collectionsGroup
