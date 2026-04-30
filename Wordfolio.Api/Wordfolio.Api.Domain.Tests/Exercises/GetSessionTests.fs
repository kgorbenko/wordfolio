module Wordfolio.Api.Domain.Tests.Exercises.GetSessionTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Operations

type TestEnv
    (
        getExerciseSession: ExerciseSessionId -> Task<ExerciseSession option>,
        getExerciseSessionEntries: ExerciseSessionId -> Task<ExerciseSessionEntry list>,
        getAttemptsBySession: ExerciseSessionId -> Task<SessionAttempt list>
    ) =
    let getExerciseSessionCalls =
        ResizeArray<ExerciseSessionId>()

    let getExerciseSessionEntriesCalls =
        ResizeArray<ExerciseSessionId>()

    let getAttemptsBySessionCalls =
        ResizeArray<ExerciseSessionId>()

    member _.GetExerciseSessionCalls =
        getExerciseSessionCalls |> Seq.toList

    member _.GetExerciseSessionEntriesCalls =
        getExerciseSessionEntriesCalls
        |> Seq.toList

    member _.GetAttemptsBySessionCalls =
        getAttemptsBySessionCalls |> Seq.toList

    interface IGetExerciseSession with
        member _.GetExerciseSession sessionId =
            getExerciseSessionCalls.Add(sessionId)
            getExerciseSession sessionId

    interface IGetExerciseSessionEntries with
        member _.GetExerciseSessionEntries sessionId =
            getExerciseSessionEntriesCalls.Add(sessionId)
            getExerciseSessionEntries sessionId

    interface IGetAttemptsBySession with
        member _.GetAttemptsBySession sessionId =
            getAttemptsBySessionCalls.Add(sessionId)
            getAttemptsBySession sessionId

let timestamp =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeSession sessionId userId exerciseType =
    { Id = ExerciseSessionId sessionId
      UserId = UserId userId
      ExerciseType = exerciseType
      CreatedAt = timestamp }

let makeSessionEntry sessionId entryId displayOrder promptData =
    { SessionId = ExerciseSessionId sessionId
      EntryId = EntryId entryId
      DisplayOrder = displayOrder
      PromptData = PromptData promptData
      PromptSchemaVersion = 1s }

let makeAttempt entryId rawAnswer isCorrect =
    { EntryId = EntryId entryId
      RawAnswer = RawAnswer rawAnswer
      IsCorrect = isCorrect
      AttemptedAt = timestamp }

[<Fact>]
let ``returns NotFound when session does not exist``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 99

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult None),
                getExerciseSessionEntries = (fun _ -> failwith "Should not be called"),
                getAttemptsBySession = (fun _ -> failwith "Should not be called")
            )

        let! result = getSession env userId sessionId

        Assert.Equal(Error GetSessionError.NotFound, result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Empty(env.GetExerciseSessionEntriesCalls)
        Assert.Empty(env.GetAttemptsBySessionCalls)
    }

[<Fact>]
let ``does not call GetExerciseSessionEntries or GetAttemptsBySession when session not found``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 99

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult None),
                getExerciseSessionEntries = (fun _ -> failwith "Should not be called"),
                getAttemptsBySession = (fun _ -> failwith "Should not be called")
            )

        let! _ = getSession env userId sessionId

        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Empty(env.GetExerciseSessionEntriesCalls)
        Assert.Empty(env.GetAttemptsBySessionCalls)
    }

[<Fact>]
let ``returns NotFound when session belongs to a different user``() =
    task {
        let userId = UserId 1
        let otherUserId = UserId 2
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 (UserId.value otherUserId) ExerciseType.Translation

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> failwith "Should not be called"),
                getAttemptsBySession = (fun _ -> failwith "Should not be called")
            )

        let! result = getSession env userId sessionId

        Assert.Equal(Error GetSessionError.NotFound, result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Empty(env.GetExerciseSessionEntriesCalls)
        Assert.Empty(env.GetAttemptsBySessionCalls)
    }

[<Fact>]
let ``does not call GetExerciseSessionEntries or GetAttemptsBySession when session belongs to different user``() =
    task {
        let userId = UserId 1
        let otherUserId = UserId 2
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 (UserId.value otherUserId) ExerciseType.Translation

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> failwith "Should not be called"),
                getAttemptsBySession = (fun _ -> failwith "Should not be called")
            )

        let! _ = getSession env userId sessionId

        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Empty(env.GetExerciseSessionEntriesCalls)
        Assert.Empty(env.GetAttemptsBySessionCalls)
    }

[<Fact>]
let ``returns Ok bundle when session is found and owned``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 1 ExerciseType.Translation

        let sessionEntry =
            makeSessionEntry 42 10 0 """{"entryText":"word","acceptedTranslations":["trans"]}"""

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> Task.FromResult([ sessionEntry ])),
                getAttemptsBySession = (fun _ -> Task.FromResult [])
            )

        let! result = getSession env userId sessionId

        Assert.True(Result.isOk result)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionCalls)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetExerciseSessionEntriesCalls)
        Assert.Equal<ExerciseSessionId list>([ sessionId ], env.GetAttemptsBySessionCalls)
    }

[<Fact>]
let ``assembles bundle entries with Attempt = None when no attempts recorded``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 1 ExerciseType.Translation

        let sessionEntry =
            makeSessionEntry 42 10 0 """{"entryText":"word","acceptedTranslations":["trans"]}"""

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> Task.FromResult([ sessionEntry ])),
                getAttemptsBySession = (fun _ -> Task.FromResult [])
            )

        let! result = getSession env userId sessionId

        let expected =
            Ok
                { SessionId = ExerciseSessionId 42
                  ExerciseType = ExerciseType.Translation
                  Entries =
                    [ { EntryId = EntryId 10
                        DisplayOrder = 0
                        PromptData = PromptData """{"entryText":"word","acceptedTranslations":["trans"]}"""
                        Attempt = None } ] }

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``assembles bundle entry with Attempt = Some when attempt exists``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 1 ExerciseType.Translation

        let sessionEntry =
            makeSessionEntry 42 10 0 """{"entryText":"word","acceptedTranslations":["trans"]}"""

        let attempt = makeAttempt 10 "trans" true

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> Task.FromResult([ sessionEntry ])),
                getAttemptsBySession = (fun _ -> Task.FromResult([ attempt ]))
            )

        let! result = getSession env userId sessionId

        let expected =
            Ok
                { SessionId = ExerciseSessionId 42
                  ExerciseType = ExerciseType.Translation
                  Entries =
                    [ { EntryId = EntryId 10
                        DisplayOrder = 0
                        PromptData = PromptData """{"entryText":"word","acceptedTranslations":["trans"]}"""
                        Attempt =
                          Some
                              { RawAnswer = RawAnswer "trans"
                                IsCorrect = true
                                AttemptedAt = timestamp } } ] }

        Assert.Equal(expected, result)
    }

[<Fact>]
let ``preserves entry DisplayOrder from session entries``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 1 ExerciseType.Translation

        let entry1 =
            makeSessionEntry 42 10 3 """{"entryText":"a","acceptedTranslations":["x"]}"""

        let entry2 =
            makeSessionEntry 42 20 7 """{"entryText":"b","acceptedTranslations":["y"]}"""

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> Task.FromResult([ entry1; entry2 ])),
                getAttemptsBySession = (fun _ -> Task.FromResult [])
            )

        let! result = getSession env userId sessionId

        let bundleEntries =
            result
            |> Result.map _.Entries
            |> Result.defaultValue []

        Assert.Equal(2, bundleEntries.Length)
        Assert.Equal(3, bundleEntries.[0].DisplayOrder)
        Assert.Equal(7, bundleEntries.[1].DisplayOrder)
    }

[<Fact>]
let ``multiple entries with only some having attempts are correctly mapped Some/None per entry``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 1 ExerciseType.Translation

        let entry1 =
            makeSessionEntry 42 10 0 """{"entryText":"a","acceptedTranslations":["x"]}"""

        let entry2 =
            makeSessionEntry 42 20 1 """{"entryText":"b","acceptedTranslations":["y"]}"""

        let entry3 =
            makeSessionEntry 42 30 2 """{"entryText":"c","acceptedTranslations":["z"]}"""

        let attempt1 = makeAttempt 10 "x" true
        let attempt3 = makeAttempt 30 "wrong" false

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> Task.FromResult([ entry1; entry2; entry3 ])),
                getAttemptsBySession = (fun _ -> Task.FromResult([ attempt1; attempt3 ]))
            )

        let! result = getSession env userId sessionId

        let bundleEntries =
            result
            |> Result.map _.Entries
            |> Result.defaultValue []

        Assert.Equal(3, bundleEntries.Length)
        Assert.True(bundleEntries.[0].Attempt.IsSome)
        Assert.True(bundleEntries.[0].Attempt.Value.IsCorrect)
        Assert.True(bundleEntries.[1].Attempt.IsNone)
        Assert.True(bundleEntries.[2].Attempt.IsSome)
        Assert.False(bundleEntries.[2].Attempt.Value.IsCorrect)
    }

[<Fact>]
let ``returns correct ExerciseType from session data``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42

        let session =
            makeSession 42 1 ExerciseType.MultipleChoice

        let env =
            TestEnv(
                getExerciseSession = (fun _ -> Task.FromResult(Some session)),
                getExerciseSessionEntries = (fun _ -> Task.FromResult []),
                getAttemptsBySession = (fun _ -> Task.FromResult [])
            )

        let! result = getSession env userId sessionId

        let expected =
            Ok
                { SessionId = ExerciseSessionId 42
                  ExerciseType = ExerciseType.MultipleChoice
                  Entries = [] }

        Assert.Equal(expected, result)
    }
