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
open Wordfolio.Api.Domain.Entries.Operations
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
    { VocabularyId: int
      EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list }

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

let private toExampleInput(req: ExampleRequest) : ExampleInput =
    { ExampleText = req.ExampleText
      Source = toExampleSourceDomain req.Source }

let private toDefinitionInput(req: DefinitionRequest) : DefinitionInput =
    { DefinitionText = req.DefinitionText
      Source = toDefinitionSourceDomain req.Source
      Examples = req.Examples |> List.map toExampleInput }

let private toTranslationInput(req: TranslationRequest) : TranslationInput =
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

let private toResponse(entry: Entry) : EntryResponse =
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

let private toErrorResponse(error: EntryError) : IResult =
    match error with
    | EntryNotFound _ -> Results.NotFound()
    | EntryTextRequired -> Results.BadRequest({| error = "Entry text is required" |})
    | EntryTextTooLong maxLength ->
        Results.BadRequest({| error = $"Entry text must be at most {maxLength} characters" |})
    | VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})
    | DuplicateEntry existingId ->
        Results.Conflict({| error = $"Duplicate entry exists with ID {EntryId.value existingId}" |})
    | NoDefinitionsOrTranslations -> Results.BadRequest({| error = "At least one definition or translation required" |})
    | TooManyExamples maxCount -> Results.BadRequest({| error = $"Too many examples (max {maxCount} per item)" |})
    | ExampleTextTooLong maxLength ->
        Results.BadRequest({| error = $"Example text must be at most {maxLength} characters" |})

let mapEntriesByVocabularyEndpoint(app: IEndpointRouteBuilder) =
    app
        .MapGet(
            "/vocabularies/{vocabularyId}/entries",
            Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
                (fun vocabularyId user dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            let env =
                                TransactionalEnv(dataSource, cancellationToken)

                            let! result = getByVocabularyId env (UserId userId) (VocabularyId vocabularyId)

                            return
                                match result with
                                | Ok entries ->
                                    let response =
                                        entries |> List.map toResponse

                                    Results.Ok(response)
                                | Error error -> toErrorResponse error
                    })
        )
        .WithTags("Entries")
    |> ignore

let mapEntriesEndpoints(group: RouteGroupBuilder) =
    group.MapPost(
        UrlTokens.Root,
        Func<CreateEntryRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun request user dataSource cancellationToken ->
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
                            create
                                env
                                (UserId userId)
                                (VocabularyId request.VocabularyId)
                                request.EntryText
                                definitions
                                translations
                                DateTimeOffset.UtcNow

                        return
                            match result with
                            | Ok entry -> Results.Created(Urls.entryById(EntryId.value entry.Id), toResponse entry)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    group.MapGet(
        UrlTokens.ById,
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun id user dataSource cancellationToken ->
            task {
                match getUserId user with
                | None -> return Results.Unauthorized()
                | Some userId ->
                    let env =
                        TransactionalEnv(dataSource, cancellationToken)

                    let! result = getById env (UserId userId) (EntryId id)

                    return
                        match result with
                        | Ok entry -> Results.Ok(toResponse entry)
                        | Error error -> toErrorResponse error
            })
    )
    |> ignore
