module Wordfolio.Api.Api.Exercises.Types

open System
open System.Text.Json
open System.Text.Json.Serialization

type ExerciseTypeDto =
    | MultipleChoice = 0
    | Translation = 1

type WorstKnownScopeRequest =
    { Type: string
      VocabularyId: string option
      CollectionId: string option }

type EntrySelectorRequest =
    { Type: string
      VocabularyId: string option
      CollectionId: string option
      EntryIds: string list option
      Count: int option
      Scope: WorstKnownScopeRequest option }

type CreateSessionRequest =
    { [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      ExerciseType: ExerciseTypeDto
      Selector: EntrySelectorRequest }

type SubmitAttemptRequest = { RawAnswer: string }

type AttemptSummaryResponse =
    { RawAnswer: string
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type SessionBundleEntryResponse =
    { EntryId: string
      DisplayOrder: int
      PromptData: JsonElement
      Attempt: AttemptSummaryResponse option }

type SessionBundleResponse =
    { SessionId: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      ExerciseType: ExerciseTypeDto
      Entries: SessionBundleEntryResponse list }

type SubmitAttemptResponse = { IsCorrect: bool }
