module Wordfolio.Api.Domain.Tests.Collections.GetByIdTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations

type TestEnv(getCollectionById: CollectionId -> Task<Collection option>) =
    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

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
let ``returns collection when found and owned by user``() =
    task {
        let collection =
            makeCollection 1 1 "Test Collection"

        let env =
            TestEnv(fun id ->
                if id <> CollectionId 1 then
                    failwith $"Unexpected id: {id}"

                Task.FromResult(Some collection))

        let! result =
            getById
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1 }

        Assert.Equal(Ok collection, result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
    }

[<Fact>]
let ``returns NotFound when collection does not exist``() =
    task {
        let env =
            TestEnv(fun id ->
                if id <> CollectionId 1 then
                    failwith $"Unexpected id: {id}"

                Task.FromResult(None))

        let! result =
            getById
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1 }

        Assert.Equal(Error(GetCollectionByIdError.CollectionNotFound(CollectionId 1)), result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection owned by different user``() =
    task {
        let collection =
            makeCollection 1 2 "Test Collection"

        let env =
            TestEnv(fun id ->
                if id <> CollectionId 1 then
                    failwith $"Unexpected id: {id}"

                Task.FromResult(Some collection))

        let! result =
            getById
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1 }

        Assert.Equal(Error(GetCollectionByIdError.CollectionAccessDenied(CollectionId 1)), result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
    }
