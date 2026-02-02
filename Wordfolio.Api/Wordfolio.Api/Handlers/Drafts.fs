module Wordfolio.Api.Handlers.Drafts

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Drafts
open Wordfolio.Api.Handlers.Entries
open Wordfolio.Api.Infrastructure.Environment

module Urls = Wordfolio.Api.Urls.Drafts

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

let mapDraftsEndpoints(endpoints: IEndpointRouteBuilder) =
    endpoints
        .MapGet(
            Urls.Path,
            Func<ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>(fun user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        let! result = Operations.get env (UserId userId)

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
        .WithTags("Drafts")
        .Produces<DraftsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    endpoints
