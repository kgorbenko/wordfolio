module Wordfolio.Api.Api.Entries.Handlers

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api
open Wordfolio.Api.Api.Entries.Mappers
open Wordfolio.Api.Api.Entries.Types
open Wordfolio.Api.Api.Helpers
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations
open Wordfolio.Api.Infrastructure.Environment

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Entries

let private toGetByVocabularyIdErrorResponse(error: GetEntriesByVocabularyIdError) : IResult =
    match error with
    | GetEntriesByVocabularyIdError.VocabularyNotFoundOrAccessDenied _ ->
        Results.NotFound({| error = "Vocabulary not found" |})

let private toCreateErrorResponse(error: CreateEntryError) : IResult =
    match error with
    | CreateEntryError.EntryTextRequired -> Results.BadRequest({| error = "Entry text is required" |})
    | CreateEntryError.EntryTextTooLong maxLength ->
        Results.BadRequest({| error = $"Entry text must be at most {maxLength} characters" |})
    | CreateEntryError.VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})
    | CreateEntryError.DuplicateEntry existingEntry ->
        Results.Conflict(
            {| error = "A matching entry already exists in this vocabulary"
               existingEntry = toEntryResponse existingEntry |}
        )
    | CreateEntryError.NoDefinitionsOrTranslations ->
        Results.BadRequest({| error = "At least one definition or translation required" |})
    | CreateEntryError.TooManyExamples maxCount ->
        Results.BadRequest({| error = $"Too many examples (max {maxCount} per item)" |})
    | CreateEntryError.ExampleTextTooLong maxLength ->
        Results.BadRequest({| error = $"Example text must be at most {maxLength} characters" |})

let private toGetByIdErrorResponse(error: GetEntryByIdError) : IResult =
    match error with
    | GetEntryByIdError.EntryNotFound _ -> Results.NotFound()
    | GetEntryByIdError.VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})

let private toDeleteErrorResponse(error: DeleteEntryError) : IResult =
    match error with
    | DeleteEntryError.EntryNotFound _ -> Results.NotFound()
    | DeleteEntryError.VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})

let private toUpdateErrorResponse(error: UpdateEntryError) : IResult =
    match error with
    | UpdateEntryError.EntryNotFound _ -> Results.NotFound()
    | UpdateEntryError.EntryTextRequired -> Results.BadRequest({| error = "Entry text is required" |})
    | UpdateEntryError.EntryTextTooLong maxLength ->
        Results.BadRequest({| error = $"Entry text must be at most {maxLength} characters" |})
    | UpdateEntryError.VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})
    | UpdateEntryError.NoDefinitionsOrTranslations ->
        Results.BadRequest({| error = "At least one definition or translation required" |})
    | UpdateEntryError.TooManyExamples maxCount ->
        Results.BadRequest({| error = $"Too many examples (max {maxCount} per item)" |})
    | UpdateEntryError.ExampleTextTooLong maxLength ->
        Results.BadRequest({| error = $"Example text must be at most {maxLength} characters" |})

let private toMoveErrorResponse(error: MoveEntryError) : IResult =
    match error with
    | MoveEntryError.EntryNotFound _ -> Results.NotFound()
    | MoveEntryError.VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})

let mapEntriesEndpoints(group: RouteGroupBuilder) =
    group
        .MapGet(
            UrlTokens.Root,
            Func<int, int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId vocabularyId user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result =
                                getByVocabularyId
                                    env
                                    { UserId = UserId userId
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId vocabularyId }

                            return
                                match result with
                                | Ok entries ->
                                    let response =
                                        entries |> List.map toEntryResponse

                                    Results.Ok(response)
                                | Error error -> toGetByVocabularyIdErrorResponse error
                    })
        )
        .Produces<EntryResponse list>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPost(
            UrlTokens.Root,
            Func<int, int, CreateEntryRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId vocabularyId request user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let parameters: CreateParameters =
                                { UserId = UserId userId
                                  CollectionId = CollectionId collectionId
                                  VocabularyId = VocabularyId vocabularyId
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
                                    Results.Created(
                                        Urls.entryById(collectionId, vocabularyId, EntryId.value entry.Id),
                                        toEntryResponse entry
                                    )
                                | Error error -> toCreateErrorResponse error
                    })
        )
        .Produces<EntryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
    |> ignore

    group
        .MapGet(
            UrlTokens.ById,
            Func<int, int, int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId vocabularyId id user dataSource cancellationToken ->
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
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId vocabularyId
                                      EntryId = EntryId id }

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toGetByIdErrorResponse error
                    })
        )
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapDelete(
            UrlTokens.ById,
            Func<int, int, int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId vocabularyId id user dataSource cancellationToken ->
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
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId vocabularyId
                                      EntryId = EntryId id }

                            return
                                match result with
                                | Ok() -> Results.NoContent()
                                | Error error -> toDeleteErrorResponse error
                    })
        )
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPut(
            UrlTokens.ById,
            Func<int, int, int, UpdateEntryRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId vocabularyId id request user dataSource cancellationToken ->
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
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId vocabularyId
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
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPost(
            UrlTokens.ById + "/move",
            Func<int, int, int, MoveEntryRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun collectionId vocabularyId id request user dataSource cancellationToken ->
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
                                      CollectionId = CollectionId collectionId
                                      VocabularyId = VocabularyId vocabularyId
                                      EntryId = EntryId id
                                      TargetVocabularyId = VocabularyId request.VocabularyId
                                      UpdatedAt = DateTimeOffset.UtcNow }

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toMoveErrorResponse error
                    })
        )
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore
