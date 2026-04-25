module Wordfolio.Api.Domain.Tests.Exercises.GetSessionTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Operations

type TestEnv(getSessionBundle: UserId -> ExerciseSessionId -> Task<SessionBundle option>) =
    let getSessionBundleCalls =
        ResizeArray<UserId * ExerciseSessionId>()

    member _.GetSessionBundleCalls =
        getSessionBundleCalls |> Seq.toList

    interface IGetSessionBundle with
        member _.GetSessionBundle userId sessionId =
            getSessionBundleCalls.Add((userId, sessionId))
            getSessionBundle userId sessionId

let makeBundle sessionId exerciseType =
    { SessionId = ExerciseSessionId sessionId
      ExerciseType = exerciseType
      Entries = [] }

[<Fact>]
let ``returns Ok bundle when session is found``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 42

        let bundle =
            makeBundle 42 ExerciseType.Translation

        let env =
            TestEnv(fun _ _ -> Task.FromResult(Some bundle))

        let! result = getSession env userId sessionId

        Assert.Equal(Ok bundle, result)
        Assert.Equal<(UserId * ExerciseSessionId) list>([ (userId, sessionId) ], env.GetSessionBundleCalls)
    }

[<Fact>]
let ``returns NotFound when session is not found``() =
    task {
        let userId = UserId 1
        let sessionId = ExerciseSessionId 99

        let env =
            TestEnv(fun _ _ -> Task.FromResult(None))

        let! result = getSession env userId sessionId

        Assert.Equal(Error GetSessionError.NotFound, result)
        Assert.Equal<(UserId * ExerciseSessionId) list>([ (userId, sessionId) ], env.GetSessionBundleCalls)
    }
