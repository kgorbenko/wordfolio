module Wordfolio.Api.Handlers.Entries

open System
open System.Security.Claims
open System.Text.Json.Serialization
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations
open Wordfolio.Api.Infrastructure.Environment

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Entries

type DefinitionSourceDto =
    | Api = 0
    | Manual = 1

type TranslationSourceDto =
    | Api = 0
    | Manual = 1

type ExampleSourceDto =
    | Api = 0
    | Custom = 1

type ExampleRequest =
    { ExampleText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: ExampleSourceDto }

type DefinitionRequest =
    { DefinitionText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: DefinitionSourceDto
      Examples: ExampleRequest list }

type TranslationRequest =
    { TranslationText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: TranslationSourceDto
      Examples: ExampleRequest list }

type CreateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }

type UpdateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list }

type MoveEntryRequest = { VocabularyId: int }

type ExampleResponse =
    { Id: int
      ExampleText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: ExampleSourceDto }

type DefinitionResponse =
    { Id: int
      DefinitionText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: DefinitionSourceDto
      DisplayOrder: int
      Examples: ExampleResponse list }

type TranslationResponse =
    { Id: int
      TranslationText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: TranslationSourceDto
      DisplayOrder: int
      Examples: ExampleResponse list }

type EntryResponse =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Definitions: DefinitionResponse list
      Translations: TranslationResponse list }

let private toDefinitionSourceDomain(source: DefinitionSourceDto) : DefinitionSource =
    match source with
    | DefinitionSourceDto.Api -> DefinitionSource.Api
    | DefinitionSourceDto.Manual -> DefinitionSource.Manual
    | _ -> DefinitionSource.Manual

let private toTranslationSourceDomain(source: TranslationSourceDto) : TranslationSource =
    match source with
    | TranslationSourceDto.Api -> TranslationSource.Api
    | TranslationSourceDto.Manual -> TranslationSource.Manual
    | _ -> TranslationSource.Manual

let private toExampleSourceDomain(source: ExampleSourceDto) : ExampleSource =
    match source with
    | ExampleSourceDto.Api -> ExampleSource.Api
    | ExampleSourceDto.Custom -> ExampleSource.Custom
    | _ -> ExampleSource.Custom

let private toExampleSourceDto(source: ExampleSource) : ExampleSourceDto =
    match source with
    | ExampleSource.Api -> ExampleSourceDto.Api
    | ExampleSource.Custom -> ExampleSourceDto.Custom

let private toDefinitionSourceDto(source: DefinitionSource) : DefinitionSourceDto =
    match source with
    | DefinitionSource.Api -> DefinitionSourceDto.Api
    | DefinitionSource.Manual -> DefinitionSourceDto.Manual

let private toTranslationSourceDto(source: TranslationSource) : TranslationSourceDto =
    match source with
    | TranslationSource.Api -> TranslationSourceDto.Api
    | TranslationSource.Manual -> TranslationSourceDto.Manual

let toExampleInput(req: ExampleRequest) : ExampleInput =
    { ExampleText = req.ExampleText
      Source = toExampleSourceDomain req.Source }

let toDefinitionInput(req: DefinitionRequest) : DefinitionInput =
    { DefinitionText = req.DefinitionText
      Source = toDefinitionSourceDomain req.Source
      Examples = req.Examples |> List.map toExampleInput }

let toTranslationInput(req: TranslationRequest) : TranslationInput =
    { TranslationText = req.TranslationText
      Source = toTranslationSourceDomain req.Source
      Examples = req.Examples |> List.map toExampleInput }

let private toExampleResponse(ex: Example) : ExampleResponse =
    { Id = ExampleId.value ex.Id
      ExampleText = ex.ExampleText
      Source = toExampleSourceDto ex.Source }

let private toDefinitionResponse(def: Definition) : DefinitionResponse =
    { Id = DefinitionId.value def.Id
      DefinitionText = def.DefinitionText
      Source = toDefinitionSourceDto def.Source
      DisplayOrder = def.DisplayOrder
      Examples =
        def.Examples
        |> List.map toExampleResponse }

let private toTranslationResponse(trans: Translation) : TranslationResponse =
    { Id = TranslationId.value trans.Id
      TranslationText = trans.TranslationText
      Source = toTranslationSourceDto trans.Source
      DisplayOrder = trans.DisplayOrder
      Examples =
        trans.Examples
        |> List.map toExampleResponse }

let toEntryResponse(entry: Entry) : EntryResponse =
    { Id = EntryId.value entry.Id
      VocabularyId = VocabularyId.value entry.VocabularyId
      EntryText = entry.EntryText
      CreatedAt = entry.CreatedAt
      UpdatedAt = entry.UpdatedAt
      Definitions =
        entry.Definitions
        |> List.map toDefinitionResponse
      Translations =
        entry.Translations
        |> List.map toTranslationResponse }

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

let toErrorResponse(error: EntryError) : IResult =
    match error with
    | EntryNotFound _ -> Results.NotFound()
    | EntryTextRequired -> Results.BadRequest({| error = "Entry text is required" |})
    | EntryTextTooLong maxLength ->
        Results.BadRequest({| error = $"Entry text must be at most {maxLength} characters" |})
    | VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})
    | DuplicateEntry existingEntry ->
        Results.Conflict(
            {| error = "A matching entry already exists in this vocabulary"
               existingEntry = toEntryResponse existingEntry |}
        )
    | NoDefinitionsOrTranslations -> Results.BadRequest({| error = "At least one definition or translation required" |})
    | TooManyExamples maxCount -> Results.BadRequest({| error = $"Too many examples (max {maxCount} per item)" |})
    | ExampleTextTooLong maxLength ->
        Results.BadRequest({| error = $"Example text must be at most {maxLength} characters" |})

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
                                    (UserId userId)
                                    (CollectionId collectionId)
                                    (VocabularyId vocabularyId)

                            return
                                match result with
                                | Ok entries ->
                                    let response =
                                        entries |> List.map toEntryResponse

                                    Results.Ok(response)
                                | Error error -> toErrorResponse error
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

                            let parameters: CreateEntryParameters =
                                { UserId = UserId userId
                                  VocabularyId = Some(VocabularyId vocabularyId)
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

                            let! result = create env (CollectionId collectionId) parameters

                            return
                                match result with
                                | Ok entry ->
                                    Results.Created(
                                        Urls.entryById(collectionId, vocabularyId, EntryId.value entry.Id),
                                        toEntryResponse entry
                                    )
                                | Error error -> toErrorResponse error
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
                                    (UserId userId)
                                    (CollectionId collectionId)
                                    (VocabularyId vocabularyId)
                                    (EntryId id)

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toErrorResponse error
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
                                    (UserId userId)
                                    (CollectionId collectionId)
                                    (VocabularyId vocabularyId)
                                    (EntryId id)

                            return
                                match result with
                                | Ok() -> Results.NoContent()
                                | Error error -> toErrorResponse error
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
                                    (UserId userId)
                                    (CollectionId collectionId)
                                    (VocabularyId vocabularyId)
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
                                    (UserId userId)
                                    (CollectionId collectionId)
                                    (VocabularyId vocabularyId)
                                    (EntryId id)
                                    (VocabularyId request.VocabularyId)
                                    DateTimeOffset.UtcNow

                            return
                                match result with
                                | Ok entry -> Results.Ok(toEntryResponse entry)
                                | Error error -> toErrorResponse error
                    })
        )
        .Produces<EntryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore
