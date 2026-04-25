module Wordfolio.Api.Domain.Exercises.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Capabilities

let createSession env (parameters: CreateSessionParameters) : Task<Result<SessionBundle, CreateSessionError>> =
    task {
        let! selectorResult = resolveEntrySelector env parameters.UserId parameters.Selector

        match selectorResult with
        | Error selectorError -> return Error(CreateSessionError.SelectorFailed selectorError)
        | Ok [] -> return Error CreateSessionError.NoEntriesResolved
        | Ok resolvedEntryIds ->
            let cappedEntryIds =
                resolvedEntryIds
                |> List.truncate Limits.MaxSessionEntries

            let! allEntries = getEntriesByIds env cappedEntryIds

            let orderedEntries =
                cappedEntryIds
                |> List.choose(fun entryId ->
                    allEntries
                    |> List.tryFind(fun entry -> entry.Id = entryId))
                |> List.filter(fun entry -> not entry.Translations.IsEmpty)

            match orderedEntries with
            | [] -> return Error CreateSessionError.NoEntriesResolved
            | _ ->

                let entries =
                    orderedEntries
                    |> List.mapi(fun index entry ->
                        let prompt =
                            Dispatch.generatePrompt parameters.ExerciseType entry

                        (entry.Id, index, prompt.PromptData, prompt.PromptSchemaVersion))

                let sessionData: CreateExerciseSessionData =
                    { UserId = parameters.UserId
                      ExerciseType = parameters.ExerciseType
                      Entries = entries
                      CreatedAt = parameters.CreatedAt }

                let! bundle = createExerciseSession env sessionData
                return Ok bundle
    }

let getSession env (userId: UserId) (sessionId: ExerciseSessionId) : Task<Result<SessionBundle, GetSessionError>> =
    task {
        let! maybeBundle = getSessionBundle env userId sessionId

        return
            match maybeBundle with
            | None -> Error GetSessionError.NotFound
            | Some bundle -> Ok bundle
    }

let submitAttempt
    env
    (userId: UserId)
    (sessionId: ExerciseSessionId)
    (entryId: EntryId)
    (rawAnswer: RawAnswer)
    (attemptedAt: DateTimeOffset)
    : Task<Result<SubmitAttemptResult, SubmitAttemptError>> =
    task {
        let! maybeSession = getExerciseSession env sessionId

        match maybeSession with
        | None -> return Error SubmitAttemptError.SessionNotFound
        | Some session when session.UserId <> userId -> return Error SubmitAttemptError.SessionNotFound
        | Some session ->
            let! maybeSessionEntry = getExerciseSessionEntry env sessionId entryId

            match maybeSessionEntry with
            | None -> return Error(SubmitAttemptError.EntryNotInSession entryId)
            | Some sessionEntry ->
                let evaluateResult =
                    Dispatch.evaluate
                        session.ExerciseType
                        sessionEntry.PromptSchemaVersion
                        sessionEntry.PromptData
                        rawAnswer

                match evaluateResult with
                | Error evaluateError -> return Error(SubmitAttemptError.EvaluateError evaluateError)
                | Ok isCorrect ->
                    let commitData: CommitAttemptData =
                        { SessionId = sessionId
                          EntryId = entryId
                          UserId = userId
                          ExerciseType = session.ExerciseType
                          PromptData = sessionEntry.PromptData
                          PromptSchemaVersion = sessionEntry.PromptSchemaVersion
                          RawAnswer = rawAnswer
                          IsCorrect = isCorrect
                          AttemptedAt = attemptedAt }

                    let! commitResult = commitAttempt env commitData

                    return
                        match commitResult with
                        | Inserted result -> Ok(Inserted result)
                        | IdempotentReplay result -> Ok(IdempotentReplay result)
                        | ConflictingReplay -> Error SubmitAttemptError.ConflictingAttempt
    }
