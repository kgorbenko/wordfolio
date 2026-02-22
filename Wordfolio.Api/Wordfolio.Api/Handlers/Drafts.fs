module Wordfolio.Api.Handlers.Drafts

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.DraftOperations
open Wordfolio.Api.Handlers.Entries
open Wordfolio.Api.Infrastructure.Environment

module EntryUrls = Wordfolio.Api.Urls.Entries
module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Drafts

type CreateDraftRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }

type VocabularyResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type DraftsResponse =
    { Vocabulary: VocabularyResponse
      Entries: EntryResponse list }

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

let mapDraftsEndpoints(group: RouteGroupBuilder) =
    group
        .MapGet(
            Urls.All,
            Func<ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result = getDrafts env (UserId userId)

                        return
                            match result with
                            | Error _ -> Results.StatusCode(StatusCodes.Status500InternalServerError)
                            | Ok None -> Results.NotFound()
                            | Ok(Some drafts) ->
                                let response: DraftsResponse =
                                    { Vocabulary =
                                        { Id = VocabularyId.value drafts.Vocabulary.Id
                                          Name = drafts.Vocabulary.Name
                                          Description = drafts.Vocabulary.Description
                                          CreatedAt = drafts.Vocabulary.CreatedAt
                                          UpdatedAt = drafts.Vocabulary.UpdatedAt }
                                      Entries =
                                        drafts.Entries
                                        |> List.map toEntryResponse }

                                Results.Ok(response)
                })
        )
        .RequireAuthorization()
        .Produces<DraftsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPost(
            UrlTokens.Root,
            Func<CreateDraftRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun request user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let parameters: CreateEntryParameters =
                                { UserId = UserId userId
                                  VocabularyId = None
                                  EntryText = request.EntryText
                                  Definitions =
                                    request.Definitions
                                    |> List.map toDefinitionInput
                                  Translations =
                                    request.Translations
                                    |> List.map toTranslationInput
                                  AllowDuplicate =
                                    request.AllowDuplicate
                                    |> Option.defaultValue false
                                  CreatedAt = DateTimeOffset.UtcNow }

                            let! result = create env parameters

                            return
                                match result with
                                | Ok entry ->
                                    Results.Created(Urls.draftById(EntryId.value entry.Id), toEntryResponse entry)
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
        .Produces<EntryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict)
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

                            let! result = getById env (UserId userId) (EntryId id)

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPut(
            UrlTokens.ById,
            Func<int, UpdateEntryRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun id request user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let definitions =
                                request.Definitions
                                |> List.map toDefinitionInput

                            let translations =
                                request.Translations
                                |> List.map toTranslationInput

                            let! result =
                                update
                                    env
                                    (UserId userId)
                                    (EntryId id)
                                    request.EntryText
                                    definitions
                                    translations
                                    DateTimeOffset.UtcNow

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
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

                            let! result = delete env (UserId userId) (EntryId id)

                            return
                                match result with
                                | Ok() -> Results.NoContent()
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPost(
            UrlTokens.ById + Urls.Move,
            Func<int, MoveEntryRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun id request user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                move
                                    env
                                    (UserId userId)
                                    (EntryId id)
                                    (VocabularyId request.VocabularyId)
                                    DateTimeOffset.UtcNow

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore
