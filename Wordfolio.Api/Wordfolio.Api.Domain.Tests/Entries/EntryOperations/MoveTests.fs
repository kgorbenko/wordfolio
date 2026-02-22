module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.MoveTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations

type MoveEntryCall =
    { EntryId: EntryId
      OldVocabularyId: VocabularyId
      NewVocabularyId: VocabularyId
      UpdatedAt: DateTimeOffset }

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: VocabularyId * CollectionId * UserId -> Task<bool>,
        hasVocabularyAccess: VocabularyId * UserId -> Task<bool>,
        moveEntry: EntryId * VocabularyId * VocabularyId * DateTimeOffset -> Task<unit>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<VocabularyId * CollectionId * UserId>()

    let hasVocabularyAccessCalls =
        ResizeArray<VocabularyId * UserId>()

    let moveEntryCalls =
        ResizeArray<MoveEntryCall>()

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
        member _.HasVocabularyAccessInCollection(vocabularyId, collectionId, userId) =
            hasVocabularyAccessInCollectionCalls.Add(vocabularyId, collectionId, userId)
            hasVocabularyAccessInCollection(vocabularyId, collectionId, userId)

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(vocabularyId, userId) =
            hasVocabularyAccessCalls.Add(vocabularyId, userId)
            hasVocabularyAccess(vocabularyId, userId)

    interface IMoveEntry with
        member _.MoveEntry(entryId, oldVocabularyId, newVocabularyId, updatedAt) =
            moveEntryCalls.Add(
                { EntryId = entryId
                  OldVocabularyId = oldVocabularyId
                  NewVocabularyId = newVocabularyId
                  UpdatedAt = updatedAt }
            )

            moveEntry(entryId, oldVocabularyId, newVocabularyId, updatedAt)

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

        let! result = move env (UserId 1) (CollectionId 5) (VocabularyId 100) (EntryId 10) (VocabularyId 200) now

        Assert.Equal(Ok movedEntry, result)
        Assert.Equal<EntryId list>([ EntryId 10; EntryId 10 ], env.GetEntryByIdCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 100, CollectionId 5, UserId 1 ],
            env.HasVocabularyAccessInCollectionCalls
        )

        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 200, UserId 1 ], env.HasVocabularyAccessCalls)

        Assert.Equal<MoveEntryCall list>(
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
                (UserId 1)
                (CollectionId 5)
                (VocabularyId 100)
                (EntryId 10)
                (VocabularyId 200)
                DateTimeOffset.UtcNow

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 100)), result)
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
                (UserId 1)
                (CollectionId 5)
                (VocabularyId 100)
                (EntryId 99)
                (VocabularyId 200)
                DateTimeOffset.UtcNow

        Assert.Equal(Error(EntryNotFound(EntryId 99)), result)
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

        let! result = move env (UserId 1) (CollectionId 5) (VocabularyId 100) (EntryId 10) (VocabularyId 200) now

        Assert.Equal(Error(EntryNotFound(EntryId 10)), result)
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

        let! result = move env (UserId 1) (CollectionId 5) (VocabularyId 100) (EntryId 10) (VocabularyId 200) now

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 200)), result)
        Assert.Equal<EntryId list>([ EntryId 10 ], env.GetEntryByIdCalls)
        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 200, UserId 1 ], env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
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

        let! result = move env (UserId 1) (CollectionId 5) (VocabularyId 100) (EntryId 10) (VocabularyId 200) now

        Assert.Equal(Ok movedEntry, result)
        Assert.Equal(1, env.MoveEntryCalls.Length)
    }
