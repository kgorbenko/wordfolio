module Wordfolio.Api.Domain.Tests.Collections.GetByUserIdTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations

type TestEnv(getCollectionsByUserId: UserId -> Task<Collection list>) =
    let getCollectionsByUserIdCalls =
        ResizeArray<UserId>()

    member _.GetCollectionsByUserIdCalls =
        getCollectionsByUserIdCalls
        |> Seq.toList

    interface IGetCollectionsByUserId with
        member _.GetCollectionsByUserId(userId) =
            getCollectionsByUserIdCalls.Add(userId)
            getCollectionsByUserId userId

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeCollection id userId name =
    { Id = CollectionId id
      UserId = UserId userId
      Name = name
      Description = None
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None }

[<Fact>]
let ``returns collections for user``() =
    task {
        let collections =
            [ makeCollection 1 1 "Collection 1"; makeCollection 2 1 "Collection 2" ]

        let env =
            TestEnv(fun userId ->
                if userId <> UserId 1 then
                    failwith $"Unexpected userId: {userId}"

                Task.FromResult(collections))

        let! result = getByUserId env (UserId 1)

        Assert.Equal<Collection list>(collections, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetCollectionsByUserIdCalls)
    }

[<Fact>]
let ``returns empty list when user has no collections``() =
    task {
        let env =
            TestEnv(fun userId ->
                if userId <> UserId 1 then
                    failwith $"Unexpected userId: {userId}"

                Task.FromResult([]))

        let! result = getByUserId env (UserId 1)

        Assert.Empty(result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetCollectionsByUserIdCalls)
    }
