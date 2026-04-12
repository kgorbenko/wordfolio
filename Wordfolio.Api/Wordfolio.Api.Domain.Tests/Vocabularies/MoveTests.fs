module Wordfolio.Api.Domain.Tests.Vocabularies.MoveTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Capabilities
open Wordfolio.Api.Domain.Vocabularies.Operations

type TestEnv
    (
        getVocabularyById: VocabularyId -> Task<Vocabulary option>,
        getCollectionById: CollectionId -> Task<Collection option>,
        moveVocabulary: MoveVocabularyData -> Task<Result<unit, unit>>
    ) =
    let getVocabularyByIdCalls =
        ResizeArray<VocabularyId>()

    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let moveVocabularyCalls =
        ResizeArray<MoveVocabularyData>()

    member _.GetVocabularyByIdCalls =
        getVocabularyByIdCalls |> Seq.toList

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    member _.MoveVocabularyCalls =
        moveVocabularyCalls |> Seq.toList

    interface IGetVocabularyById with
        member _.GetVocabularyById(id) =
            getVocabularyByIdCalls.Add(id)
            getVocabularyById id

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface IMoveVocabulary with
        member _.MoveVocabulary(data) =
            moveVocabularyCalls.Add(data)
            moveVocabulary data

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeCollection id userId =
    let createdAt =
        DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

    { Id = CollectionId id
      UserId = UserId userId
      Name = "Test Collection"
      Description = None
      CreatedAt = createdAt
      UpdatedAt = createdAt }

let makeVocabulary id collectionId name =
    let createdAt =
        DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = name
      Description = None
      CreatedAt = createdAt
      UpdatedAt = createdAt }

[<Fact>]
let ``moves vocabulary when both collections are owned and vocabulary is in source``() =
    task {
        let now =
            DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

        let sourceCollection = makeCollection 10 1
        let targetCollection = makeCollection 20 1

        let existingVocabulary =
            makeVocabulary 5 10 "My Vocab"

        let movedVocabulary =
            { existingVocabulary with
                CollectionId = CollectionId 20
                UpdatedAt = now }

        let getVocabularyByIdCallCount = ref 0

        let env =
            TestEnv(
                getVocabularyById =
                    (fun _ ->
                        let callIndex =
                            getVocabularyByIdCallCount.Value

                        getVocabularyByIdCallCount.Value <- callIndex + 1

                        if callIndex = 0 then
                            Task.FromResult(Some existingVocabulary)
                        elif callIndex = 1 then
                            Task.FromResult(Some movedVocabulary)
                        else
                            failwith "Unexpected getVocabularyById call"),
                getCollectionById =
                    (fun id ->
                        if id = CollectionId 10 then
                            Task.FromResult(Some sourceCollection)
                        elif id = CollectionId 20 then
                            Task.FromResult(Some targetCollection)
                        else
                            failwith $"Unexpected collection id: {id}"),
                moveVocabulary = (fun _ -> Task.FromResult(Ok()))
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 10
                  VocabularyId = VocabularyId 5
                  TargetCollectionId = CollectionId 20
                  UpdatedAt = now }

        Assert.Equal(Ok movedVocabulary, result)

        Assert.Equal<CollectionId list>([ CollectionId 10; CollectionId 20 ], env.GetCollectionByIdCalls)
        Assert.Equal<VocabularyId list>([ VocabularyId 5; VocabularyId 5 ], env.GetVocabularyByIdCalls)

        Assert.Equal<MoveVocabularyData list>(
            [ { VocabularyId = VocabularyId 5
                OldCollectionId = CollectionId 10
                NewCollectionId = CollectionId 20
                UpdatedAt = now } ],
            env.MoveVocabularyCalls
        )
    }

[<Fact>]
let ``returns CollectionNotFoundOrAccessDenied when source collection belongs to different user``() =
    task {
        let sourceCollection = makeCollection 10 2

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some sourceCollection)),
                moveVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 10
                  VocabularyId = VocabularyId 5
                  TargetCollectionId = CollectionId 20
                  UpdatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) }

        Assert.Equal(Error(MoveVocabularyError.CollectionNotFoundOrAccessDenied(CollectionId 10)), result)

        Assert.Equal<CollectionId list>([ CollectionId 10 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetVocabularyByIdCalls)
        Assert.Empty(env.MoveVocabularyCalls)
    }

[<Fact>]
let ``returns CollectionNotFoundOrAccessDenied when source collection does not exist``() =
    task {
        let env =
            TestEnv(
                getVocabularyById = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(None)),
                moveVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 10
                  VocabularyId = VocabularyId 5
                  TargetCollectionId = CollectionId 20
                  UpdatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) }

        Assert.Equal(Error(MoveVocabularyError.CollectionNotFoundOrAccessDenied(CollectionId 10)), result)

        Assert.Equal<CollectionId list>([ CollectionId 10 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetVocabularyByIdCalls)
        Assert.Empty(env.MoveVocabularyCalls)
    }

[<Fact>]
let ``returns VocabularyNotFound when vocabulary is not in source collection``() =
    task {
        let sourceCollection = makeCollection 10 1

        let vocabularyInOtherCollection =
            makeVocabulary 5 99 "My Vocab"

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some vocabularyInOtherCollection)),
                getCollectionById = (fun _ -> Task.FromResult(Some sourceCollection)),
                moveVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 10
                  VocabularyId = VocabularyId 5
                  TargetCollectionId = CollectionId 20
                  UpdatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) }

        Assert.Equal(Error(MoveVocabularyError.VocabularyNotFound(VocabularyId 5)), result)

        Assert.Equal<CollectionId list>([ CollectionId 10 ], env.GetCollectionByIdCalls)
        Assert.Equal<VocabularyId list>([ VocabularyId 5 ], env.GetVocabularyByIdCalls)
        Assert.Empty(env.MoveVocabularyCalls)
    }

[<Fact>]
let ``returns CollectionNotFoundOrAccessDenied when target collection belongs to different user``() =
    task {
        let sourceCollection = makeCollection 10 1
        let targetCollection = makeCollection 20 2

        let existingVocabulary =
            makeVocabulary 5 10 "My Vocab"

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById =
                    (fun id ->
                        if id = CollectionId 10 then
                            Task.FromResult(Some sourceCollection)
                        elif id = CollectionId 20 then
                            Task.FromResult(Some targetCollection)
                        else
                            failwith $"Unexpected collection id: {id}"),
                moveVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 10
                  VocabularyId = VocabularyId 5
                  TargetCollectionId = CollectionId 20
                  UpdatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) }

        Assert.Equal(Error(MoveVocabularyError.CollectionNotFoundOrAccessDenied(CollectionId 20)), result)

        Assert.Equal<CollectionId list>([ CollectionId 10; CollectionId 20 ], env.GetCollectionByIdCalls)
        Assert.Equal<VocabularyId list>([ VocabularyId 5 ], env.GetVocabularyByIdCalls)
        Assert.Empty(env.MoveVocabularyCalls)
    }

[<Fact>]
let ``returns CollectionNotFoundOrAccessDenied when target collection does not exist``() =
    task {
        let sourceCollection = makeCollection 10 1

        let existingVocabulary =
            makeVocabulary 5 10 "My Vocab"

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById =
                    (fun id ->
                        if id = CollectionId 10 then
                            Task.FromResult(Some sourceCollection)
                        else
                            Task.FromResult(None)),
                moveVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 10
                  VocabularyId = VocabularyId 5
                  TargetCollectionId = CollectionId 20
                  UpdatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) }

        Assert.Equal(Error(MoveVocabularyError.CollectionNotFoundOrAccessDenied(CollectionId 20)), result)

        Assert.Equal<CollectionId list>([ CollectionId 10; CollectionId 20 ], env.GetCollectionByIdCalls)
        Assert.Equal<VocabularyId list>([ VocabularyId 5 ], env.GetVocabularyByIdCalls)
        Assert.Empty(env.MoveVocabularyCalls)
    }

[<Fact>]
let ``returns VocabularyNotFound when move capability returns Error``() =
    task {
        let now =
            DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

        let sourceCollection = makeCollection 10 1
        let targetCollection = makeCollection 20 1

        let existingVocabulary =
            makeVocabulary 5 10 "My Vocab"

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById =
                    (fun id ->
                        if id = CollectionId 10 then
                            Task.FromResult(Some sourceCollection)
                        elif id = CollectionId 20 then
                            Task.FromResult(Some targetCollection)
                        else
                            failwith $"Unexpected collection id: {id}"),
                moveVocabulary = (fun _ -> Task.FromResult(Error()))
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 10
                  VocabularyId = VocabularyId 5
                  TargetCollectionId = CollectionId 20
                  UpdatedAt = now }

        Assert.Equal(Error(MoveVocabularyError.VocabularyNotFound(VocabularyId 5)), result)

        Assert.Equal<CollectionId list>([ CollectionId 10; CollectionId 20 ], env.GetCollectionByIdCalls)
        Assert.Equal<VocabularyId list>([ VocabularyId 5 ], env.GetVocabularyByIdCalls)

        Assert.Equal<MoveVocabularyData list>(
            [ { VocabularyId = VocabularyId 5
                OldCollectionId = CollectionId 10
                NewCollectionId = CollectionId 20
                UpdatedAt = now } ],
            env.MoveVocabularyCalls
        )
    }
