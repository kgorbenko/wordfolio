module Wordfolio.Api.Handlers.Collections

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections

module Urls =
    [<Literal>]
    let Collections = "/collections"

    [<Literal>]
    let CollectionById = "/collections/{id:int}"

type CollectionResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateCollectionRequest = { Name: string; Description: string option }

type UpdateCollectionRequest = { Name: string; Description: string option }

let private toResponse(collection: Collection) : CollectionResponse =
    { Id = CollectionId.value collection.Id
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt }

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

let private toErrorResponse(error: CollectionError) : IResult =
    match error with
    | CollectionNotFound _ -> Results.NotFound()
    | CollectionAccessDenied _ -> Results.Forbid()
    | CollectionNameRequired -> Results.BadRequest({| error = "Name is required" |})
    | CollectionNameTooLong maxLength ->
        Results.BadRequest({| error = $"Name must be at most {maxLength} characters" |})

let mapCollectionsEndpoints(app: IEndpointRouteBuilder) =
    app
        .MapGet(
            Urls.Collections,
            Func<ClaimsPrincipal, ICollectionRepository, CancellationToken, _>(fun user repository cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized() :> IResult
                    | Some userId ->
                        let! collections =
                            getByUserIdAsync repository (UserId userId) cancellationToken

                        let response = collections |> List.map toResponse
                        return Results.Ok(response) :> IResult
                })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapGet(
            Urls.CollectionById,
            Func<int, ClaimsPrincipal, ICollectionRepository, CancellationToken, _>
                (fun id user repository cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let! result =
                                getByIdAsync repository (UserId userId) (CollectionId id) cancellationToken

                            return
                                match result with
                                | Ok collection -> Results.Ok(toResponse collection) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPost(
            Urls.Collections,
            Func<CreateCollectionRequest, ClaimsPrincipal, ICollectionRepository, CancellationToken, _>
                (fun request user repository cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let command: CreateCollectionCommand =
                                { UserId = UserId userId
                                  Name = request.Name
                                  Description = request.Description }

                            let! result =
                                createAsync repository command DateTimeOffset.UtcNow cancellationToken

                            return
                                match result with
                                | Ok collection -> Results.Created($"/collections/{CollectionId.value collection.Id}", toResponse collection) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapPut(
            Urls.CollectionById,
            Func<int, UpdateCollectionRequest, ClaimsPrincipal, ICollectionRepository, CancellationToken, _>
                (fun id request user repository cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let command: UpdateCollectionCommand =
                                { CollectionId = CollectionId id
                                  UserId = UserId userId
                                  Name = request.Name
                                  Description = request.Description }

                            let! result =
                                updateAsync repository command DateTimeOffset.UtcNow cancellationToken

                            return
                                match result with
                                | Ok collection -> Results.Ok(toResponse collection) :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
        .MapDelete(
            Urls.CollectionById,
            Func<int, ClaimsPrincipal, ICollectionRepository, CancellationToken, _>
                (fun id user repository cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized() :> IResult
                        | Some userId ->
                            let command: DeleteCollectionCommand =
                                { CollectionId = CollectionId id
                                  UserId = UserId userId }

                            let! result = deleteAsync repository command cancellationToken

                            return
                                match result with
                                | Ok() -> Results.NoContent() :> IResult
                                | Error error -> toErrorResponse error
                    })
        )
        .RequireAuthorization()
    |> ignore

    app
