module Wordfolio.Api.Handlers.Vocabularies

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies

module Urls =
    [<Literal>]
    let VocabulariesByCollection = "/collections/{collectionId:int}/vocabularies"

    [<Literal>]
    let VocabularyById = "/vocabularies/{id:int}"

type VocabularyResponse =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateVocabularyRequest = { Name: string; Description: string option }

type UpdateVocabularyRequest = { Name: string; Description: string option }

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
    app
        .MapGet(
            Urls.VocabulariesByCollection,
            Func<int, ClaimsPrincipal, IVocabularyRepository, ICollectionRepository, CancellationToken, _>
                (fun collectionId user vocabularyRepo collectionRepo cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let! result =
                                getByCollectionIdAsync
                                    vocabularyRepo
                                    collectionRepo
                                    (UserId userId)
                                    (CollectionId collectionId)
                                    cancellationToken

                            return
                                match result with
                                | Ok vocabularies ->
                                    let response = vocabularies |> List.map toResponse
                                    Results.Ok(response) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapGet(
            Urls.VocabularyById,
            Func<int, ClaimsPrincipal, IVocabularyRepository, ICollectionRepository, CancellationToken, _>
                (fun id user vocabularyRepo collectionRepo cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let! result =
                                getByIdAsync
                                    vocabularyRepo
                                    collectionRepo
                                    (UserId userId)
                                    (VocabularyId id)
                                    cancellationToken

                            return
                                match result with
                                | Ok vocabulary -> Results.Ok(toResponse vocabulary) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPost(
            Urls.VocabulariesByCollection,
            Func<int, CreateVocabularyRequest, ClaimsPrincipal, IVocabularyRepository, ICollectionRepository, CancellationToken, _>
                (fun collectionId request user vocabularyRepo collectionRepo cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let command: CreateVocabularyCommand =
                                { UserId = UserId userId
                                  CollectionId = CollectionId collectionId
                                  Name = request.Name
                                  Description = request.Description }

                            let! result =
                                createAsync vocabularyRepo collectionRepo command DateTimeOffset.UtcNow cancellationToken

                            return
                                match result with
                                | Ok vocabulary ->
                                    Results.Created(
                                        $"/vocabularies/{VocabularyId.value vocabulary.Id}",
                                        toResponse vocabulary
                                    )
                                    :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPut(
            Urls.VocabularyById,
            Func<int, UpdateVocabularyRequest, ClaimsPrincipal, IVocabularyRepository, ICollectionRepository, CancellationToken, _>
                (fun id request user vocabularyRepo collectionRepo cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let command: UpdateVocabularyCommand =
                                { UserId = UserId userId
                                  VocabularyId = VocabularyId id
                                  Name = request.Name
                                  Description = request.Description }

                            let! result =
                                updateAsync vocabularyRepo collectionRepo command DateTimeOffset.UtcNow cancellationToken

                            return
                                match result with
                                | Ok vocabulary -> Results.Ok(toResponse vocabulary) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapDelete(
            Urls.VocabularyById,
            Func<int, ClaimsPrincipal, IVocabularyRepository, ICollectionRepository, CancellationToken, _>
                (fun id user vocabularyRepo collectionRepo cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let command: DeleteVocabularyCommand =
                                { UserId = UserId userId
                                  VocabularyId = VocabularyId id }

                            let! result =
                                deleteAsync vocabularyRepo collectionRepo command cancellationToken

                            return
                                match result with
                                | Ok() -> Results.NoContent() :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
