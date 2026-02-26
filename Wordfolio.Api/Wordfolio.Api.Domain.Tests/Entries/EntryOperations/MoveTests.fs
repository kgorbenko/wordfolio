module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.MoveTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: HasVocabularyAccessInCollectionData -> Task<bool>,
        hasVocabularyAccess: VocabularyId * UserId -> Task<bool>,
        moveEntry: MoveEntryData -> Task<unit>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<HasVocabularyAccessInCollectionData>()

    let hasVocabularyAccessCalls =
        ResizeArray<VocabularyId * UserId>()

    let moveEntryCalls =
        ResizeArray<MoveEntryData>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.HasVocabularyAccessInCollectionCalls =
        hasVocabularyAccessInCollectionCalls
        |> Seq.toList

    member _.HasVocabularyAccessCalls =
        hasVocabularyAccessCalls |> Seq.toList

    member _.MoveEntryCalls =
        moveEntryCalls |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(data) =
            hasVocabularyAccessInCollectionCalls.Add(data)
            hasVocabularyAccessInCollection data

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(vocabularyId, userId) =
            hasVocabularyAccessCalls.Add(vocabularyId, userId)
            hasVocabularyAccess(vocabularyId, userId)

    interface IMoveEntry with
        member _.MoveEntry(data) =
            moveEntryCalls.Add(data)
            moveEntry data

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeEntry id vocabularyId text createdAt updatedAt =
    { Id = EntryId id
      VocabularyId = VocabularyId vocabularyId
      EntryText = text
      CreatedAt = createdAt
      UpdatedAt = updatedAt
      Definitions = []
      Translations = [] }

[<Fact>]
let ``moves entry when vocabulary is in collection and entry belongs to vocabulary``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "hello" now None

        let movedEntry =
            makeEntry 10 200 "hello" now (Some now)

        let getEntryByIdCallCount = ref 0

        let env =
            TestEnv(
                getEntryById =
                    (fun _ ->
                        let callIndex = getEntryByIdCallCount.Value
                        getEntryByIdCallCount.Value <- callIndex + 1

                        if callIndex = 0 then
                            Task.FromResult(Some existingEntry)
                        elif callIndex = 1 then
                            Task.FromResult(Some movedEntry)
                        else
                            failwith "Unexpected getEntryById call"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true)),
                moveEntry = (fun _ -> Task.FromResult(()))
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = VocabularyId 200
                  UpdatedAt = now }

        Assert.Equal(Ok movedEntry, result)
        Assert.Equal<EntryId list>([ EntryId 10; EntryId 10 ], env.GetEntryByIdCalls)

        Assert.Equal<HasVocabularyAccessInCollectionData list>(
            [ ({ VocabularyId = VocabularyId 100
                 CollectionId = CollectionId 5
                 UserId = UserId 1 }
              : HasVocabularyAccessInCollectionData) ],
            env.HasVocabularyAccessInCollectionCalls
        )

        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 200, UserId 1 ], env.HasVocabularyAccessCalls)

        Assert.Equal<MoveEntryData list>(
            [ { EntryId = EntryId 10
                OldVocabularyId = VocabularyId 100
                NewVocabularyId = VocabularyId 200
                UpdatedAt = now } ],
            env.MoveEntryCalls
        )
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when source vocabulary is not in collection``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(false)),
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                moveEntry = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = VocabularyId 200
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(MoveEntryError.VocabularyNotFoundOrAccessDenied(VocabularyId 100)), result)
        Assert.Empty(env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
    }

[<Fact>]
let ``returns EntryNotFound when entry does not exist``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                moveEntry = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 99
                  TargetVocabularyId = VocabularyId 200
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(MoveEntryError.EntryNotFound(EntryId 99)), result)
        Assert.Equal<EntryId list>([ EntryId 99 ], env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
    }

[<Fact>]
let ``returns EntryNotFound when entry belongs to different vocabulary``() =
    task {
        let now = DateTimeOffset.UtcNow

        let entry =
            makeEntry 10 999 "hello" now None

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                moveEntry = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = VocabularyId 200
                  UpdatedAt = now }

        Assert.Equal(Error(MoveEntryError.EntryNotFound(EntryId 10)), result)
        Assert.Equal<EntryId list>([ EntryId 10 ], env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when target vocabulary access is denied``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "hello" now None

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some existingEntry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(false)),
                moveEntry = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = VocabularyId 200
                  UpdatedAt = now }

        Assert.Equal(Error(MoveEntryError.VocabularyNotFoundOrAccessDenied(VocabularyId 200)), result)
        Assert.Equal<EntryId list>([ EntryId 10 ], env.GetEntryByIdCalls)
        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 200, UserId 1 ], env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
    }

[<Fact>]
let ``throws when post-move entry fetch returns None``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "hello" now None

        let getEntryByIdCallCount = ref 0

        let env =
            TestEnv(
                getEntryById =
                    (fun _ ->
                        let callIndex = getEntryByIdCallCount.Value
                        getEntryByIdCallCount.Value <- callIndex + 1

                        if callIndex = 0 then
                            Task.FromResult(Some existingEntry)
                        else
                            Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true)),
                moveEntry = (fun _ -> Task.FromResult(()))
            )

        let! ex =
            Assert.ThrowsAsync<Exception>(fun () ->
                move
                    env
                    { UserId = UserId 1
                      CollectionId = CollectionId 5
                      VocabularyId = VocabularyId 100
                      EntryId = EntryId 10
                      TargetVocabularyId = VocabularyId 200
                      UpdatedAt = now }
                :> Task)

        Assert.Equal("Entry EntryId 10 not found after move", ex.Message)
    }

[<Fact>]
let ``move succeeds without duplicate checks in target vocabulary``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "duplicate-text" now None

        let movedEntry =
            makeEntry 10 200 "duplicate-text" now (Some now)

        let getEntryByIdCallCount = ref 0

        let env =
            TestEnv(
                getEntryById =
                    (fun _ ->
                        let callIndex = getEntryByIdCallCount.Value
                        getEntryByIdCallCount.Value <- callIndex + 1

                        if callIndex = 0 then
                            Task.FromResult(Some existingEntry)
                        elif callIndex = 1 then
                            Task.FromResult(Some movedEntry)
                        else
                            failwith "Unexpected getEntryById call"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true)),
                moveEntry = (fun _ -> Task.FromResult(()))
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = VocabularyId 200
                  UpdatedAt = now }

        Assert.Equal(Ok movedEntry, result)
        Assert.Equal(1, env.MoveEntryCalls.Length)
    }
