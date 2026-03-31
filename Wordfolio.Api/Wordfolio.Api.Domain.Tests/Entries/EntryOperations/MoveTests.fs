module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.MoveTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations
open Wordfolio.Api.Domain.Operations

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: HasVocabularyAccessInCollectionData -> Task<bool>,
        hasVocabularyAccess: HasVocabularyAccessData -> Task<bool>,
        moveEntry: MoveEntryData -> Task<unit>,
        getDefaultVocabulary: UserId -> Task<Vocabulary option>,
        createDefaultVocabulary: CreateDefaultVocabularyParameters -> Task<VocabularyId>,
        getDefaultCollection: UserId -> Task<Collection option>,
        createDefaultCollection: CreateDefaultCollectionParameters -> Task<CollectionId>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<HasVocabularyAccessInCollectionData>()

    let hasVocabularyAccessCalls =
        ResizeArray<HasVocabularyAccessData>()

    let moveEntryCalls =
        ResizeArray<MoveEntryData>()

    let getDefaultVocabularyCalls =
        ResizeArray<UserId>()

    let createDefaultVocabularyCalls =
        ResizeArray<CreateDefaultVocabularyParameters>()

    let getDefaultCollectionCalls =
        ResizeArray<UserId>()

    let createDefaultCollectionCalls =
        ResizeArray<CreateDefaultCollectionParameters>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.HasVocabularyAccessInCollectionCalls =
        hasVocabularyAccessInCollectionCalls
        |> Seq.toList

    member _.HasVocabularyAccessCalls =
        hasVocabularyAccessCalls |> Seq.toList

    member _.MoveEntryCalls =
        moveEntryCalls |> Seq.toList

    member _.GetDefaultVocabularyCalls =
        getDefaultVocabularyCalls |> Seq.toList

    member _.CreateDefaultVocabularyCalls =
        createDefaultVocabularyCalls
        |> Seq.toList

    member _.GetDefaultCollectionCalls =
        getDefaultCollectionCalls |> Seq.toList

    member _.CreateDefaultCollectionCalls =
        createDefaultCollectionCalls
        |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(data) =
            hasVocabularyAccessInCollectionCalls.Add(data)
            hasVocabularyAccessInCollection data

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(data) =
            hasVocabularyAccessCalls.Add(data)
            hasVocabularyAccess data

    interface IMoveEntry with
        member _.MoveEntry(data) =
            moveEntryCalls.Add(data)
            moveEntry data

    interface IGetDefaultVocabulary with
        member _.GetDefaultVocabulary(userId) =
            getDefaultVocabularyCalls.Add(userId)
            getDefaultVocabulary userId

    interface ICreateDefaultVocabulary with
        member _.CreateDefaultVocabulary(parameters) =
            createDefaultVocabularyCalls.Add(parameters)
            createDefaultVocabulary parameters

    interface IGetDefaultCollection with
        member _.GetDefaultCollection(userId) =
            getDefaultCollectionCalls.Add(userId)
            getDefaultCollection userId

    interface ICreateDefaultCollection with
        member _.CreateDefaultCollection(parameters) =
            createDefaultCollectionCalls.Add(parameters)
            createDefaultCollection parameters

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

let makeCollection id userId : Collection =
    { Id = CollectionId id
      UserId = UserId userId
      Name = SystemCollectionName
      Description = None
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None }

let makeVocabulary id collectionId createdAt : Vocabulary =
    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = DefaultVocabularyName
      Description = None
      CreatedAt = createdAt
      UpdatedAt = None }

[<Fact>]
let ``moves entry when explicit target vocabulary is accessible``() =
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
                moveEntry = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = Some(VocabularyId 200)
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

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = VocabularyId 200
                 UserId = UserId 1 }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Equal<MoveEntryData list>(
            [ { EntryId = EntryId 10
                OldVocabularyId = VocabularyId 100
                NewVocabularyId = VocabularyId 200
                UpdatedAt = now } ],
            env.MoveEntryCalls
        )

        Assert.Empty(env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
    }

[<Fact>]
let ``moves entry to existing default vocabulary when target vocabulary id is None``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "hello" now None

        let existingDefaultVocabulary =
            makeVocabulary 300 20 now

        let movedEntry =
            makeEntry 10 300 "hello" now (Some now)

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
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                moveEntry = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some existingDefaultVocabulary)),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = None
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

        Assert.Empty(env.HasVocabularyAccessCalls)

        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)

        Assert.Equal<MoveEntryData list>(
            [ { EntryId = EntryId 10
                OldVocabularyId = VocabularyId 100
                NewVocabularyId = VocabularyId 300
                UpdatedAt = now } ],
            env.MoveEntryCalls
        )
    }

[<Fact>]
let ``creates default vocabulary when target vocabulary id is None and default collection exists``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "hello" now None

        let existingDefaultCollection =
            makeCollection 55 1

        let movedEntry =
            makeEntry 10 300 "hello" now (Some now)

        let expectedVocabularyParams: CreateDefaultVocabularyParameters =
            { CollectionId = CollectionId 55
              Name = DefaultVocabularyName
              Description = None
              CreatedAt = now }

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
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                moveEntry = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> Task.FromResult(None)),
                createDefaultVocabulary = (fun _ -> Task.FromResult(VocabularyId 300)),
                getDefaultCollection = (fun _ -> Task.FromResult(Some existingDefaultCollection)),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = None
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

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultVocabularyCalls)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)

        Assert.Equal<CreateDefaultVocabularyParameters list>(
            [ expectedVocabularyParams ],
            env.CreateDefaultVocabularyCalls
        )

        Assert.Equal<MoveEntryData list>(
            [ { EntryId = EntryId 10
                OldVocabularyId = VocabularyId 100
                NewVocabularyId = VocabularyId 300
                UpdatedAt = now } ],
            env.MoveEntryCalls
        )
    }

[<Fact>]
let ``creates default collection and vocabulary when target vocabulary id is None and neither exists``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "hello" now None

        let movedEntry =
            makeEntry 10 300 "hello" now (Some now)

        let expectedCollectionParams: CreateDefaultCollectionParameters =
            { UserId = UserId 1
              Name = SystemCollectionName
              Description = None
              CreatedAt = now }

        let expectedVocabularyParams: CreateDefaultVocabularyParameters =
            { CollectionId = CollectionId 55
              Name = DefaultVocabularyName
              Description = None
              CreatedAt = now }

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
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                moveEntry = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> Task.FromResult(None)),
                createDefaultVocabulary = (fun _ -> Task.FromResult(VocabularyId 300)),
                getDefaultCollection = (fun _ -> Task.FromResult(None)),
                createDefaultCollection = (fun _ -> Task.FromResult(CollectionId 55))
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = None
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

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultVocabularyCalls)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultCollectionCalls)

        Assert.Equal<CreateDefaultCollectionParameters list>(
            [ expectedCollectionParams ],
            env.CreateDefaultCollectionCalls
        )

        Assert.Equal<CreateDefaultVocabularyParameters list>(
            [ expectedVocabularyParams ],
            env.CreateDefaultVocabularyCalls
        )

        Assert.Equal<MoveEntryData list>(
            [ { EntryId = EntryId 10
                OldVocabularyId = VocabularyId 100
                NewVocabularyId = VocabularyId 300
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
                moveEntry = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = Some(VocabularyId 200)
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(MoveEntryError.VocabularyNotFoundOrAccessDenied(VocabularyId 100)), result)
        Assert.Empty(env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
        Assert.Empty(env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
    }

[<Fact>]
let ``returns EntryNotFound when entry does not exist``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                moveEntry = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 99
                  TargetVocabularyId = Some(VocabularyId 200)
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(MoveEntryError.EntryNotFound(EntryId 99)), result)
        Assert.Equal<EntryId list>([ EntryId 99 ], env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
        Assert.Empty(env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
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
                moveEntry = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = Some(VocabularyId 200)
                  UpdatedAt = now }

        Assert.Equal(Error(MoveEntryError.EntryNotFound(EntryId 10)), result)
        Assert.Equal<EntryId list>([ EntryId 10 ], env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.MoveEntryCalls)
        Assert.Empty(env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when explicit target vocabulary access is denied``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 10 100 "hello" now None

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some existingEntry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(false)),
                moveEntry = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = Some(VocabularyId 200)
                  UpdatedAt = now }

        Assert.Equal(Error(MoveEntryError.VocabularyNotFoundOrAccessDenied(VocabularyId 200)), result)
        Assert.Equal<EntryId list>([ EntryId 10 ], env.GetEntryByIdCalls)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = VocabularyId 200
                 UserId = UserId 1 }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.MoveEntryCalls)
        Assert.Empty(env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
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
                moveEntry = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! ex =
            Assert.ThrowsAsync<Exception>(fun () ->
                move
                    env
                    { UserId = UserId 1
                      CollectionId = CollectionId 5
                      VocabularyId = VocabularyId 100
                      EntryId = EntryId 10
                      TargetVocabularyId = Some(VocabularyId 200)
                      UpdatedAt = now }
                :> Task)

        Assert.Equal("Entry EntryId 10 not found after move", ex.Message)
    }

[<Fact>]
let ``move succeeds without duplicate checks in explicit target vocabulary``() =
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
                moveEntry = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            move
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 100
                  EntryId = EntryId 10
                  TargetVocabularyId = Some(VocabularyId 200)
                  UpdatedAt = now }

        Assert.Equal(Ok movedEntry, result)
        Assert.Equal(1, env.MoveEntryCalls.Length)
        Assert.Empty(env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
    }
