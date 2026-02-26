module Wordfolio.Api.Domain.Tests.Collections.DeleteTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations

type TestEnv(getCollectionById: CollectionId -> Task<Collection option>, deleteCollection: CollectionId -> Task<int>) =
    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let deleteCollectionCalls =
        ResizeArray<CollectionId>()

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    member _.DeleteCollectionCalls =
        deleteCollectionCalls |> Seq.toList

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface IDeleteCollection with
        member _.DeleteCollection(id) =
            deleteCollectionCalls.Add(id)
            deleteCollection id

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
let ``deletes collection when owned by user``() =
    task {
        let existingCollection =
            makeCollection 1 1 "Test Collection"

        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(Some existingCollection)),
                deleteCollection =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(1))
            )

        let! result =
            delete
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1 }

        Assert.Equal(Ok(), result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.DeleteCollectionCalls)
    }

[<Fact>]
let ``returns NotFound when collection does not exist``() =
    task {
        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(None)),
                deleteCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            delete
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1 }

        Assert.Equal(Error(DeleteCollectionError.CollectionNotFound(CollectionId 1)), result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.DeleteCollectionCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection owned by different user``() =
    task {
        let existingCollection =
            makeCollection 1 2 "Test Collection"

        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(Some existingCollection)),
                deleteCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            delete
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1 }

        Assert.Equal(Error(DeleteCollectionError.CollectionAccessDenied(CollectionId 1)), result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.DeleteCollectionCalls)
    }

[<Fact>]
let ``returns NotFound when delete affects no rows``() =
    task {
        let existingCollection =
            makeCollection 1 1 "Test Collection"

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                deleteCollection = (fun _ -> Task.FromResult(0))
            )

        let! result =
            delete
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1 }

        Assert.Equal(Error(DeleteCollectionError.CollectionNotFound(CollectionId 1)), result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.DeleteCollectionCalls)
    }
