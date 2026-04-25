namespace Wordfolio.Api.Domain.Exercises

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type CreateExerciseSessionData =
    { UserId: UserId
      ExerciseType: ExerciseType
      Entries: (EntryId * int * PromptData * int16) list
      CreatedAt: DateTimeOffset }

type CommitAttemptData =
    { SessionId: ExerciseSessionId
      EntryId: EntryId
      UserId: UserId
      ExerciseType: ExerciseType
      PromptData: PromptData
      PromptSchemaVersion: int16
      RawAnswer: RawAnswer
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type IResolveEntrySelector =
    abstract ResolveEntrySelector: UserId -> EntrySelector -> Task<Result<EntryId list, SelectorError>>

type IGetEntriesByIds =
    abstract GetEntriesByIds: EntryId list -> Task<Entry list>

type ICreateExerciseSession =
    abstract CreateExerciseSession: CreateExerciseSessionData -> Task<SessionBundle>

type IGetExerciseSession =
    abstract GetExerciseSession: ExerciseSessionId -> Task<ExerciseSession option>

type IGetExerciseSessionEntry =
    abstract GetExerciseSessionEntry: ExerciseSessionId -> EntryId -> Task<ExerciseSessionEntry option>

type IGetSessionBundle =
    abstract GetSessionBundle: UserId -> ExerciseSessionId -> Task<SessionBundle option>

type ICommitAttempt =
    abstract CommitAttempt: CommitAttemptData -> Task<SubmitAttemptResult>

module Capabilities =
    let resolveEntrySelector (env: #IResolveEntrySelector) userId selector =
        env.ResolveEntrySelector userId selector

    let getEntriesByIds (env: #IGetEntriesByIds) entryIds = env.GetEntriesByIds entryIds

    let createExerciseSession (env: #ICreateExerciseSession) data = env.CreateExerciseSession data

    let getExerciseSession (env: #IGetExerciseSession) sessionId = env.GetExerciseSession sessionId

    let getExerciseSessionEntry (env: #IGetExerciseSessionEntry) sessionId entryId =
        env.GetExerciseSessionEntry sessionId entryId

    let getSessionBundle (env: #IGetSessionBundle) userId sessionId = env.GetSessionBundle userId sessionId

    let commitAttempt (env: #ICommitAttempt) data = env.CommitAttempt data
