module Wordfolio.Api.Domain.Tests.Exercises.SubmitAttemptTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Operations

type TestEnv
    (
        getExerciseSession: ExerciseSessionId -> Task<ExerciseSession option>,
        getExerciseSessionEntry: ExerciseSessionId -> EntryId -> Task<ExerciseSessionEntry option>,
        commitAttempt: CommitAttemptData -> Task<SubmitAttemptResult>
    ) =
    let getExerciseSessionCalls =
        ResizeArray<ExerciseSessionId>()

    let getExerciseSessionEntryCalls =
        ResizeArray<ExerciseSessionId * EntryId>()

    let commitAttemptCalls =
        ResizeArray<CommitAttemptData>()

    member _.GetExerciseSessionCalls =
        getExerciseSessionCalls |> Seq.toList

    member _.GetExerciseSessionEntryCalls =
        getExerciseSessionEntryCalls
        |> Seq.toList

    member _.CommitAttemptCalls =
        commitAttemptCalls |> Seq.toList

    interface IGetExerciseSession with
        member _.GetExerciseSession sessionId =
            getExerciseSessionCalls.Add(sessionId)
            getExerciseSession sessionId

    interface IGetExerciseSessionEntry with
        member _.GetExerciseSessionEntry sessionId entryId =
            getExerciseSessionEntryCalls.Add((sessionId, entryId))
            getExerciseSessionEntry sessionId entryId

    interface ICommitAttempt with
        member _.CommitAttempt data =
            commitAttemptCalls.Add(data)
            commitAttempt data

let timestamp =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeSession sessionId userId exerciseType =
    { Id = sessionId
      UserId = userId
      ExerciseType = exerciseType
      CreatedAt = timestamp }

let makeSessionEntry sessionId entryId promptData schemaVersion =
    { SessionId = sessionId
      EntryId = entryId
      DisplayOrder = 0
      PromptData = promptData
      PromptSchemaVersion = schemaVersion }

let makeTranslationEntry entryId =
    let entry =
        { Id = entryId
          VocabularyId = VocabularyId 1
          EntryText = "cat"
          CreatedAt = timestamp
          UpdatedAt = timestamp
          Definitions = []
          Translations =
            [ { Id = TranslationId 1
                TranslationText = "hello"
                Source = TranslationSource.Manual
                DisplayOrder = 0
                Examples = [] } ] }

    let prompt =
        Translation.generatePrompt entry

    makeSessionEntry (ExerciseSessionId 42) entryId prompt.PromptData prompt.PromptSchemaVersion

[<Fact>]
let ``returns SessionNotFound when session does not exist``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 99
        let entryId = EntryId 1

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(None)),
                getExerciseSessionEntry = (fun _ _ -> failwith "Should not be called"),
                commitAttempt = (fun _ -> failwith "Should not be called")
            )

        let! result = submitAttempt env userId sessionId entryId (RawAnswer "hello") timestamp

        Assert.Equal(Error SubmitAttemptError.SessionNotFound, result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Empty(env.GetExerciseSessionEntryCalls)
        Assert.Empty(env.CommitAttemptCalls)
    }

[<Fact>]
let ``returns SessionNotFound when session belongs to different user``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42
        let entryId = EntryId 1

        let session =
            makeSession sessionId (UserId 99) ExerciseType.Translation

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntry = (fun _ _ -> failwith "Should not be called"),
                commitAttempt = (fun _ -> failwith "Should not be called")
            )

        let! result = submitAttempt env userId sessionId entryId (RawAnswer "hello") timestamp

        Assert.Equal(Error SubmitAttemptError.SessionNotFound, result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Empty(env.GetExerciseSessionEntryCalls)
        Assert.Empty(env.CommitAttemptCalls)
    }

[<Fact>]
let ``returns EntryNotInSession when entry does not exist in session``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42
        let entryId = EntryId 5

        let session =
            makeSession sessionId userId ExerciseType.Translation

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntry = (fun _ _ -> Task.FromResult(None)),
                commitAttempt = (fun _ -> failwith "Should not be called")
            )

        let! result = submitAttempt env userId sessionId entryId (RawAnswer "hello") timestamp

        Assert.Equal(Error(SubmitAttemptError.EntryNotInSession entryId), result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Equal<(ExerciseSessionId * EntryId) list>([ (sessionId, entryId) ], env.GetExerciseSessionEntryCalls)
        Assert.Empty(env.CommitAttemptCalls)
    }

[<Fact>]
let ``returns EvaluateError when prompt schema version is unsupported``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42
        let entryId = EntryId 1

        let session =
            makeSession sessionId userId ExerciseType.Translation

        let sessionEntry =
            makeSessionEntry sessionId entryId (PromptData "{}") 2s

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntry = (fun _ _ -> Task.FromResult(Some sessionEntry)),
                commitAttempt = (fun _ -> failwith "Should not be called")
            )

        let! result = submitAttempt env userId sessionId entryId (RawAnswer "hello") timestamp

        Assert.Equal(Error(SubmitAttemptError.EvaluateError EvaluateError.UnsupportedPromptSchemaVersion), result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Equal<(ExerciseSessionId * EntryId) list>([ (sessionId, entryId) ], env.GetExerciseSessionEntryCalls)
        Assert.Empty(env.CommitAttemptCalls)
    }

[<Fact>]
let ``returns Ok Inserted when commit succeeds with new attempt``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42
        let entryId = EntryId 1

        let attemptedAt =
            DateTimeOffset(2025, 6, 1, 10, 0, 0, TimeSpan.Zero)

        let session =
            makeSession sessionId userId ExerciseType.Translation

        let sessionEntry =
            makeTranslationEntry entryId

        let inserted =
            Inserted
                { AttemptId = ExerciseAttemptId 1
                  IsCorrect = true }

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntry = (fun _ _ -> Task.FromResult(Some sessionEntry)),
                commitAttempt = (fun _ -> Task.FromResult(inserted))
            )

        let! result = submitAttempt env userId sessionId entryId (RawAnswer "hello") attemptedAt

        Assert.Equal(Ok inserted, result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Equal<(ExerciseSessionId * EntryId) list>([ (sessionId, entryId) ], env.GetExerciseSessionEntryCalls)

        let expectedCommitData: CommitAttemptData =
            { SessionId = sessionId
              EntryId = entryId
              UserId = userId
              ExerciseType = ExerciseType.Translation
              PromptData = sessionEntry.PromptData
              PromptSchemaVersion = sessionEntry.PromptSchemaVersion
              RawAnswer = RawAnswer "hello"
              IsCorrect = true
              AttemptedAt = attemptedAt }

        Assert.Equal<CommitAttemptData list>([ expectedCommitData ], env.CommitAttemptCalls)
    }

[<Fact>]
let ``returns Ok IdempotentReplay when commit is idempotent replay``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42
        let entryId = EntryId 1

        let attemptedAt =
            DateTimeOffset(2025, 6, 1, 10, 0, 0, TimeSpan.Zero)

        let session =
            makeSession sessionId userId ExerciseType.Translation

        let sessionEntry =
            makeTranslationEntry entryId

        let idempotentReplay =
            IdempotentReplay { IsCorrect = true }

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntry = (fun _ _ -> Task.FromResult(Some sessionEntry)),
                commitAttempt = (fun _ -> Task.FromResult(idempotentReplay))
            )

        let! result = submitAttempt env userId sessionId entryId (RawAnswer "hello") attemptedAt

        Assert.Equal(Ok idempotentReplay, result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Equal<(ExerciseSessionId * EntryId) list>([ (sessionId, entryId) ], env.GetExerciseSessionEntryCalls)

        let expectedCommitData: CommitAttemptData =
            { SessionId = sessionId
              EntryId = entryId
              UserId = userId
              ExerciseType = ExerciseType.Translation
              PromptData = sessionEntry.PromptData
              PromptSchemaVersion = sessionEntry.PromptSchemaVersion
              RawAnswer = RawAnswer "hello"
              IsCorrect = true
              AttemptedAt = attemptedAt }

        Assert.Equal<CommitAttemptData list>([ expectedCommitData ], env.CommitAttemptCalls)
    }

[<Fact>]
let ``returns Error ConflictingAttempt when commit is conflicting replay``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42
        let entryId = EntryId 1

        let attemptedAt =
            DateTimeOffset(2025, 6, 1, 10, 0, 0, TimeSpan.Zero)

        let session =
            makeSession sessionId userId ExerciseType.Translation

        let sessionEntry =
            makeTranslationEntry entryId

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntry = (fun _ _ -> Task.FromResult(Some sessionEntry)),
                commitAttempt = (fun _ -> Task.FromResult(ConflictingReplay))
            )

        let! result = submitAttempt env userId sessionId entryId (RawAnswer "hello") attemptedAt

        Assert.Equal(Error SubmitAttemptError.ConflictingAttempt, result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Equal<(ExerciseSessionId * EntryId) list>([ (sessionId, entryId) ], env.GetExerciseSessionEntryCalls)

        let expectedCommitData: CommitAttemptData =
            { SessionId = sessionId
              EntryId = entryId
              UserId = userId
              ExerciseType = ExerciseType.Translation
              PromptData = sessionEntry.PromptData
              PromptSchemaVersion = sessionEntry.PromptSchemaVersion
              RawAnswer = RawAnswer "hello"
              IsCorrect = true
              AttemptedAt = attemptedAt }

        Assert.Equal<CommitAttemptData list>([ expectedCommitData ], env.CommitAttemptCalls)
    }
