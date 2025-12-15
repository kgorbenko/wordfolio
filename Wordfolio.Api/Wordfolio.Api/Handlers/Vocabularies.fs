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

let mapVocabulariesEndpoints(app: IEndpointRouteBuilder) =
    let collectionsGroup =
        app.MapGroup("/collections")

    let vocabulariesGroup =
        app.MapGroup("/vocabularies")

    collectionsGroup.MapGet(
        "/{collectionId:int}/vocabularies",
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun collectionId user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result =
                            Transactions.runInTransaction env (fun appEnv ->
                                getByCollectionId appEnv (UserId userId) (CollectionId collectionId))

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

    collectionsGroup.MapPost(
        "/{collectionId:int}/vocabularies",
        Func<int, CreateVocabularyRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun collectionId request user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
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
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    vocabulariesGroup.MapGet(
        "/{id:int}",
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun id user dataSource cancellationToken ->
            task {
                match getUserId user with
                | None -> return Results.Unauthorized()
                | Some userId ->
                    let env =
                        TransactionalEnv(dataSource, cancellationToken)

                    let! result =
                        Transactions.runInTransaction env (fun appEnv ->
                            getById appEnv (UserId userId) (VocabularyId id))

                    return
                        match result with
                        | Ok vocabulary -> Results.Ok(toResponse vocabulary)
                        | Error error -> toErrorResponse error
            })
    )
    |> ignore

    vocabulariesGroup.MapPut(
        "/{id:int}",
        Func<int, UpdateVocabularyRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun id request user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
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
                            | Ok vocabulary -> Results.Ok(toResponse vocabulary)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    vocabulariesGroup.MapDelete(
        "/{id:int}",
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun id user dataSource cancellationToken ->
            task {
                match getUserId user with
                | None -> return Results.Unauthorized()
                | Some userId ->
                    let env =
                        TransactionalEnv(dataSource, cancellationToken)

                    let! result =
                        Transactions.runInTransaction env (fun appEnv ->
                            delete appEnv (UserId userId) (VocabularyId id))

                    return
                        match result with
                        | Ok() -> Results.NoContent()
                        | Error error -> toErrorResponse error
            })
    )
    |> ignore

    app
