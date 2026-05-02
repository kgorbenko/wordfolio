module Wordfolio.Api.Api.Exercises.Handlers

open System
open System.Security.Claims
open System.Threading
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Api.Exercises.Mappers
open Wordfolio.Api.Api.Exercises.Types
open Wordfolio.Api.Api.Helpers
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Operations
open Wordfolio.Api.Infrastructure.Environment
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

module UrlTokens = Wordfolio.Api.Urls
module ExerciseUrls = Wordfolio.Api.Urls.Exercises

let private toCreateSessionErrorResponse(error: CreateSessionError) : IResult =
    match error with
    | CreateSessionError.NoEntriesResolved ->
        Results.BadRequest({| error = "No entries could be resolved for the given selector" |})
    | CreateSessionError.SelectorFailed selectorError ->
        match selectorError with
        | SelectorError.VocabularyNotOwnedByUser _ -> Results.Forbid()
        | SelectorError.CollectionNotOwnedByUser _ -> Results.Forbid()
        | SelectorError.EntryNotOwnedByUser _ -> Results.Forbid()

let private toGetSessionErrorResponse(error: GetSessionError) : IResult =
    match error with
    | GetSessionError.NotFound -> Results.NotFound()

let private toSubmitAttemptErrorResponse(error: SubmitAttemptError) : IResult =
    match error with
    | SubmitAttemptError.SessionNotFound -> Results.NotFound()
    | SubmitAttemptError.EntryNotInSession _ -> Results.NotFound()
    | SubmitAttemptError.ConflictingAttempt -> Results.Conflict({| error = "A conflicting attempt already exists" |})
    | SubmitAttemptError.EvaluateError _ -> Results.StatusCode(StatusCodes.Status500InternalServerError)

let private invalidRequestBody() : IResult =
    Results.BadRequest({| error = "Invalid request body" |})

let private createSessionHandler
    (request: CreateSessionRequest)
    (user: ClaimsPrincipal)
    (encoder: IResourceIdEncoder)
    (dataSource: NpgsqlDataSource)
    (cancellationToken: CancellationToken)
    : Task<IResult> =
    match getUserId user with
    | None -> Task.FromResult(Results.Unauthorized())
    | Some userId ->
        match request |> Option.ofObj with
        | None -> Task.FromResult(invalidRequestBody())
        | Some request ->
            match request.Selector |> Option.ofObj with
            | None -> Task.FromResult(invalidRequestBody())
            | Some selectorRequest ->
                match tryMapSelector encoder selectorRequest with
                | Error message -> Task.FromResult(Results.BadRequest({| error = message |}))
                | Ok selector ->
                    let limitError =
                        match selector with
                        | ExplicitEntries entries when entries.Length > Limits.MaxSessionEntries ->
                            Some $"Cannot specify more than {Limits.MaxSessionEntries} entries"
                        | WorstKnown(_, count) when count <= 0 -> Some "Count must be positive"
                        | WorstKnown(_, count) when count > Limits.MaxSessionEntries ->
                            Some $"Count cannot exceed {Limits.MaxSessionEntries}"
                        | _ -> None

                    match limitError with
                    | Some message -> Task.FromResult(Results.BadRequest({| error = message |}))
                    | None ->
                        let parameters: CreateSessionParameters =
                            { UserId = UserId userId
                              ExerciseType = toExerciseTypeDomain request.ExerciseType
                              Selector = selector
                              CreatedAt = DateTimeOffset.UtcNow }

                        let env =
                            TransactionalEnv(dataSource, cancellationToken)

                        task {
                            let! result = runInTransaction env (fun appEnv -> createSession appEnv parameters)

                            return
                                match result with
                                | Ok bundle ->
                                    let response =
                                        toSessionBundleResponse encoder bundle

                                    Results.Created(ExerciseUrls.sessionById response.SessionId, response)
                                | Error error -> toCreateSessionErrorResponse error
                        }

let mapExercisesEndpoints(group: RouteGroupBuilder) =
    group
        .MapPost(
            UrlTokens.Root,
            Func<CreateSessionRequest, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun request user encoder dataSource cancellationToken ->
                    createSessionHandler request user encoder dataSource cancellationToken)
        )
        .Produces<SessionBundleResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
    |> ignore

    group
        .MapGet(
            ExerciseUrls.SessionById,
            Func<string, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun sessionId user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            match encoder.Decode(sessionId) with
                            | None -> return Results.NotFound()
                            | Some decodedSessionId ->
                                let env =
                                    TransactionalEnv(dataSource, cancellationToken)

                                let! result =
                                    runInTransaction env (fun appEnv ->
                                        getSession appEnv (UserId userId) (ExerciseSessionId decodedSessionId))

                                return
                                    match result with
                                    | Ok bundle -> Results.Ok(toSessionBundleResponse encoder bundle)
                                    | Error error -> toGetSessionErrorResponse error
                    })
        )
        .Produces<SessionBundleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
    |> ignore

    group
        .MapPost(
            ExerciseUrls.EntryAttempts,
            Func<string, string, SubmitAttemptRequest, ClaimsPrincipal, IResourceIdEncoder, NpgsqlDataSource, CancellationToken, _>
                (fun sessionId entryId request user encoder dataSource cancellationToken ->
                    task {
                        match getUserId user with
                        | None -> return Results.Unauthorized()
                        | Some userId ->
                            match encoder.Decode(sessionId), encoder.Decode(entryId) with
                            | None, _
                            | _, None -> return Results.NotFound()
                            | Some decodedSessionId, Some decodedEntryId ->
                                match request |> Option.ofObj with
                                | None -> return invalidRequestBody()
                                | Some request ->
                                    match request.RawAnswer |> Option.ofObj with
                                    | None -> return invalidRequestBody()
                                    | Some rawAnswer ->
                                        let env =
                                            TransactionalEnv(dataSource, cancellationToken)

                                        let! result =
                                            runInTransaction env (fun appEnv ->
                                                submitAttempt
                                                    appEnv
                                                    (UserId userId)
                                                    (ExerciseSessionId decodedSessionId)
                                                    (EntryId decodedEntryId)
                                                    (RawAnswer rawAnswer)
                                                    DateTimeOffset.UtcNow)

                                        return
                                            match result with
                                            | Ok(Inserted inserted) ->
                                                let response: SubmitAttemptResponse =
                                                    { IsCorrect = inserted.IsCorrect }

                                                Results.Created(ExerciseUrls.sessionById sessionId, response)
                                            | Ok(IdempotentReplay replay) ->
                                                let response: SubmitAttemptResponse =
                                                    { IsCorrect = replay.IsCorrect }

                                                Results.Ok(response)
                                            | Ok ConflictingReplay ->
                                                toSubmitAttemptErrorResponse SubmitAttemptError.ConflictingAttempt
                                            | Error error -> toSubmitAttemptErrorResponse error
                    })
        )
        .Produces<SubmitAttemptResponse>(StatusCodes.Status201Created)
        .Produces<SubmitAttemptResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
    |> ignore
