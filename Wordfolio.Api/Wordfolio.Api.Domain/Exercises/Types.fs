namespace Wordfolio.Api.Domain.Exercises

open System

open Wordfolio.Api.Domain

type PromptData = | PromptData of string

type RawAnswer = | RawAnswer of string

module PromptData =
    let value(PromptData v) = v

module RawAnswer =
    let value(RawAnswer v) = v

module Limits =
    [<Literal>]
    let KnowledgeWindowSize = 10

    [<Literal>]
    let MaxSessionEntries = 10

type ExerciseType =
    | MultipleChoice
    | Translation

type ExerciseSession =
    { Id: ExerciseSessionId
      UserId: UserId
      ExerciseType: ExerciseType
      CreatedAt: DateTimeOffset }

type ExerciseSessionEntry =
    { SessionId: ExerciseSessionId
      EntryId: EntryId
      DisplayOrder: int
      PromptData: PromptData
      PromptSchemaVersion: int16 }

type AttemptSummary =
    { RawAnswer: RawAnswer
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type SessionBundleEntry =
    { EntryId: EntryId
      DisplayOrder: int
      PromptData: PromptData
      Attempt: AttemptSummary option }

type SessionBundle =
    { SessionId: ExerciseSessionId
      ExerciseType: ExerciseType
      Entries: SessionBundleEntry list }

type WorstKnownScope =
    | AllUserEntries
    | WithinVocabulary of VocabularyId
    | WithinCollection of CollectionId

type EntrySelector =
    | VocabularyScope of VocabularyId
    | CollectionScope of CollectionId
    | WorstKnown of scope: WorstKnownScope * count: int
    | ExplicitEntries of EntryId list

type CreateSessionParameters =
    { UserId: UserId
      ExerciseType: ExerciseType
      Selector: EntrySelector
      CreatedAt: DateTimeOffset }

type SubmitAttemptParameters =
    { SessionId: ExerciseSessionId
      EntryId: EntryId
      UserId: UserId
      PromptData: PromptData
      PromptSchemaVersion: int16
      RawAnswer: RawAnswer
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type AttemptInserted =
    { AttemptId: ExerciseAttemptId
      IsCorrect: bool }

type AttemptAlreadyRecorded = { IsCorrect: bool }

[<RequireQualifiedAccess>]
type EvaluateError =
    | UnsupportedPromptSchemaVersion
    | MalformedPromptData

type GeneratedPrompt =
    { PromptData: PromptData
      PromptSchemaVersion: int16 }

type SubmitAttemptResult =
    | Inserted of AttemptInserted
    | IdempotentReplay of AttemptAlreadyRecorded
    | ConflictingReplay
