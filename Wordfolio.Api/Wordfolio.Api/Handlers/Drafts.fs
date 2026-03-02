module Wordfolio.Api.Handlers.Drafts

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Api.Helpers
open Wordfolio.Api.Api.Mappers
open Wordfolio.Api.Api.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.DraftOperations
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

let private toCreateErrorResponse(error: CreateDraftEntryError) : IResult =
    match error with
    | CreateDraftEntryError.EntryTextRequired -> Results.BadRequest({| error = "Entry text is required" |})
    | CreateDraftEntryError.EntryTextTooLong maxLength ->
        Results.BadRequest({| error = $"Entry text must be at most {maxLength} characters" |})
    | CreateDraftEntryError.DuplicateEntry existingEntry ->
        Results.Conflict(
            {| error = "A matching entry already exists in this vocabulary"
               existingEntry = toEntryResponse existingEntry |}
        )
    | CreateDraftEntryError.NoDefinitionsOrTranslations ->
        Results.BadRequest({| error = "At least one definition or translation required" |})
    | CreateDraftEntryError.TooManyExamples maxCount ->
        Results.BadRequest({| error = $"Too many examples (max {maxCount} per item)" |})
    | CreateDraftEntryError.ExampleTextTooLong maxLength ->
        Results.BadRequest({| error = $"Example text must be at most {maxLength} characters" |})

let private toGetByIdErrorResponse(error: GetDraftEntryByIdError) : IResult =
    match error with
    | GetDraftEntryByIdError.EntryNotFound _ -> Results.NotFound()

let private toUpdateErrorResponse(error: UpdateDraftEntryError) : IResult =
    match error with
    | UpdateDraftEntryError.EntryNotFound _ -> Results.NotFound()
    | UpdateDraftEntryError.EntryTextRequired -> Results.BadRequest({| error = "Entry text is required" |})
    | UpdateDraftEntryError.EntryTextTooLong maxLength ->
        Results.BadRequest({| error = $"Entry text must be at most {maxLength} characters" |})
    | UpdateDraftEntryError.NoDefinitionsOrTranslations ->
        Results.BadRequest({| error = "At least one definition or translation required" |})
    | UpdateDraftEntryError.TooManyExamples maxCount ->
        Results.BadRequest({| error = $"Too many examples (max {maxCount} per item)" |})
    | UpdateDraftEntryError.ExampleTextTooLong maxLength ->
        Results.BadRequest({| error = $"Example text must be at most {maxLength} characters" |})

let private toDeleteErrorResponse(error: DeleteDraftEntryError) : IResult =
    match error with
    | DeleteDraftEntryError.EntryNotFound _ -> Results.NotFound()

let private toMoveErrorResponse(error: MoveDraftEntryError) : IResult =
    match error with
    | MoveDraftEntryError.EntryNotFound _ -> Results.NotFound()
    | MoveDraftEntryError.VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})

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

                        let! result = getDrafts env { UserId = UserId userId }

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

                            let parameters: CreateParameters =
                                { UserId = UserId userId
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
                                | Error error -> toCreateErrorResponse error
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

                            let! result =
                                getById
                                    env
                                    { UserId = UserId userId
                                      EntryId = EntryId id }

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toGetByIdErrorResponse error
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
                                    { UserId = UserId userId
                                      EntryId = EntryId id
                                      EntryText = request.EntryText
                                      Definitions = definitions
                                      Translations = translations
                                      UpdatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toUpdateErrorResponse error
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

                            let! result =
                                delete
                                    env
                                    { UserId = UserId userId
                                      EntryId = EntryId id }

                            return
                                match result with
                                | Ok() -> Results.NoContent()
                                | Error error -> toDeleteErrorResponse error
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
                                    { UserId = UserId userId
                                      EntryId = EntryId id
                                      TargetVocabularyId = VocabularyId request.VocabularyId
                                      UpdatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toMoveErrorResponse error
                    })
        )
        .RequireAuthorization()
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore
