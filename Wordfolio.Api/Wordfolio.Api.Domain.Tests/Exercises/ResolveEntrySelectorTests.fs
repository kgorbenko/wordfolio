module Wordfolio.Api.Domain.Tests.Exercises.ResolveEntrySelectorTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Operations

type TestEnv
    (
        hasVocabularyAccess: HasVocabularyAccessData -> Task<bool>,
        getCollectionById: CollectionId -> Task<Collection option>,
        getEntryIdsByVocabularyId: GetEntryIdsByVocabularyIdData -> Task<EntryId list>,
        getEntryIdsByCollectionId: GetEntryIdsByCollectionIdData -> Task<EntryId list>,
        getOwnedEntryIds: GetOwnedEntryIdsData -> Task<EntryId list>,
        getEntryIdsByUserId: UserId -> Task<EntryId list>,
        getWorstKnownEntryIds: GetWorstKnownEntryIdsData -> Task<EntryId list>
    ) =
    let hasVocabularyAccessCalls =
        ResizeArray<HasVocabularyAccessData>()

    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let getEntryIdsByVocabularyIdCalls =
        ResizeArray<GetEntryIdsByVocabularyIdData>()

    let getEntryIdsByCollectionIdCalls =
        ResizeArray<GetEntryIdsByCollectionIdData>()

    let getOwnedEntryIdsCalls =
        ResizeArray<GetOwnedEntryIdsData>()

    let getEntryIdsByUserIdCalls =
        ResizeArray<UserId>()

    let getWorstKnownEntryIdsCalls =
        ResizeArray<GetWorstKnownEntryIdsData>()

    member _.HasVocabularyAccessCalls =
        hasVocabularyAccessCalls |> Seq.toList

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    member _.GetEntryIdsByVocabularyIdCalls =
        getEntryIdsByVocabularyIdCalls
        |> Seq.toList

    member _.GetEntryIdsByCollectionIdCalls =
        getEntryIdsByCollectionIdCalls
        |> Seq.toList

    member _.GetOwnedEntryIdsCalls =
        getOwnedEntryIdsCalls |> Seq.toList

    member _.GetEntryIdsByUserIdCalls =
        getEntryIdsByUserIdCalls |> Seq.toList

    member _.GetWorstKnownEntryIdsCalls =
        getWorstKnownEntryIdsCalls |> Seq.toList

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess data =
            hasVocabularyAccessCalls.Add(data)
            hasVocabularyAccess data

    interface IGetCollectionById with
        member _.GetCollectionById collectionId =
            getCollectionByIdCalls.Add(collectionId)
            getCollectionById collectionId

    interface IGetEntryIdsByVocabularyId with
        member _.GetEntryIdsByVocabularyId data =
            getEntryIdsByVocabularyIdCalls.Add(data)
            getEntryIdsByVocabularyId data

    interface IGetEntryIdsByCollectionId with
        member _.GetEntryIdsByCollectionId data =
            getEntryIdsByCollectionIdCalls.Add(data)
            getEntryIdsByCollectionId data

    interface IGetOwnedEntryIds with
        member _.GetOwnedEntryIds data =
            getOwnedEntryIdsCalls.Add(data)
            getOwnedEntryIds data

    interface IGetEntryIdsByUserId with
        member _.GetEntryIdsByUserId userId =
            getEntryIdsByUserIdCalls.Add(userId)
            getEntryIdsByUserId userId

    interface IGetWorstKnownEntryIds with
        member _.GetWorstKnownEntryIds data =
            getWorstKnownEntryIdsCalls.Add(data)
            getWorstKnownEntryIds data

let timestamp =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeCollection id userId =
    { Id = CollectionId id
      UserId = UserId userId
      Name = "Test Collection"
      Description = None
      CreatedAt = timestamp
      UpdatedAt = timestamp }

[<Fact>]
let ``VocabularyScope returns VocabularyNotOwnedByUser when ownership check fails``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult false),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (VocabularyScope vocabularyId)

        Assert.Equal(Error(SelectorError.VocabularyNotOwnedByUser vocabularyId), result)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``VocabularyScope does not call GetEntryIdsByVocabularyId when ownership fails``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult false),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! _ = resolveEntrySelector env userId (VocabularyScope vocabularyId)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``VocabularyScope passes correct HasVocabularyAccessData to ownership check``() =
    task {
        let userId = UserId 5
        let vocabularyId = VocabularyId 20

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult false),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! _ = resolveEntrySelector env userId (VocabularyScope vocabularyId)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``VocabularyScope returns entry IDs when user owns vocabulary``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10

        let entryIds =
            [ EntryId 1; EntryId 2; EntryId 3 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult true),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> Task.FromResult entryIds),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (VocabularyScope vocabularyId)

        Assert.Equal(Ok entryIds, result)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)

        Assert.Equal<GetEntryIdsByVocabularyIdData list>(
            [ { VocabularyId = vocabularyId
                UserId = userId } ],
            env.GetEntryIdsByVocabularyIdCalls
        )

        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``VocabularyScope returns Ok empty list when owned vocabulary has no entries``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult true),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> Task.FromResult []),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (VocabularyScope vocabularyId)

        Assert.Equal(Ok [], result)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)

        Assert.Equal<GetEntryIdsByVocabularyIdData list>(
            [ { VocabularyId = vocabularyId
                UserId = userId } ],
            env.GetEntryIdsByVocabularyIdCalls
        )

        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``CollectionScope returns CollectionNotOwnedByUser when collection is None``() =
    task {
        let userId = UserId 1
        let collectionId = CollectionId 5

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult None),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (CollectionScope collectionId)

        Assert.Equal(Error(SelectorError.CollectionNotOwnedByUser collectionId), result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``CollectionScope does not call GetEntryIdsByCollectionId when collection is None``() =
    task {
        let userId = UserId 1
        let collectionId = CollectionId 5

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult None),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! _ = resolveEntrySelector env userId (CollectionScope collectionId)

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``CollectionScope returns CollectionNotOwnedByUser when collection owned by another user``() =
    task {
        let userId = UserId 1
        let otherUserId = UserId 2
        let collectionId = CollectionId 5

        let collection =
            makeCollection 5 (UserId.value otherUserId)

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (CollectionScope collectionId)

        Assert.Equal(Error(SelectorError.CollectionNotOwnedByUser collectionId), result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``CollectionScope does not call GetEntryIdsByCollectionId when collection belongs to another user``() =
    task {
        let userId = UserId 1
        let otherUserId = UserId 2
        let collectionId = CollectionId 5

        let collection =
            makeCollection 5 (UserId.value otherUserId)

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! _ = resolveEntrySelector env userId (CollectionScope collectionId)

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``CollectionScope returns entry IDs when collection is owned``() =
    task {
        let userId = UserId 1
        let collectionId = CollectionId 5
        let collection = makeCollection 5 1
        let entryIds = [ EntryId 10; EntryId 20 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> Task.FromResult entryIds),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (CollectionScope collectionId)

        Assert.Equal(Ok entryIds, result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)

        Assert.Equal<GetEntryIdsByCollectionIdData list>(
            [ { CollectionId = collectionId
                UserId = userId } ],
            env.GetEntryIdsByCollectionIdCalls
        )

        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``ExplicitEntries returns Ok with all IDs when all are owned``() =
    task {
        let userId = UserId 1

        let requestedIds =
            [ EntryId 1; EntryId 2; EntryId 3 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> Task.FromResult requestedIds),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (ExplicitEntries requestedIds)

        Assert.Equal(Ok requestedIds, result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)

        Assert.Equal<GetOwnedEntryIdsData list>(
            [ { EntryIds = requestedIds
                UserId = userId } ],
            env.GetOwnedEntryIdsCalls
        )

        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``ExplicitEntries preserves original request order in Ok result``() =
    task {
        let userId = UserId 1

        let requestedIds =
            [ EntryId 3; EntryId 1; EntryId 2 ]

        let ownedIds =
            [ EntryId 1; EntryId 2; EntryId 3 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> Task.FromResult ownedIds),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (ExplicitEntries requestedIds)

        Assert.Equal(Ok requestedIds, result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)

        Assert.Equal<GetOwnedEntryIdsData list>(
            [ { EntryIds = requestedIds
                UserId = userId } ],
            env.GetOwnedEntryIdsCalls
        )

        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``ExplicitEntries passes correct GetOwnedEntryIdsData to ownership check``() =
    task {
        let userId = UserId 7

        let requestedIds =
            [ EntryId 100; EntryId 200 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> Task.FromResult requestedIds),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! _ = resolveEntrySelector env userId (ExplicitEntries requestedIds)

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)

        Assert.Equal<GetOwnedEntryIdsData list>(
            [ { EntryIds = requestedIds
                UserId = userId } ],
            env.GetOwnedEntryIdsCalls
        )

        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``ExplicitEntries returns EntryNotOwnedByUser with exact unowned IDs``() =
    task {
        let userId = UserId 1

        let requestedIds =
            [ EntryId 1; EntryId 2; EntryId 3 ]

        let ownedIds = [ EntryId 1; EntryId 3 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> Task.FromResult ownedIds),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (ExplicitEntries requestedIds)

        Assert.Equal(Error(SelectorError.EntryNotOwnedByUser [ EntryId 2 ]), result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)

        Assert.Equal<GetOwnedEntryIdsData list>(
            [ { EntryIds = requestedIds
                UserId = userId } ],
            env.GetOwnedEntryIdsCalls
        )

        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``ExplicitEntries returns Ok empty list when input is empty without calling GetOwnedEntryIds``() =
    task {
        let userId = UserId 1

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (ExplicitEntries [])

        Assert.Equal(Ok [], result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``WorstKnown AllUserEntries calls GetEntryIdsByUserId then GetWorstKnownEntryIds``() =
    task {
        let userId = UserId 1
        let count = 5

        let allIds =
            [ EntryId 1; EntryId 2; EntryId 3 ]

        let worstIds = [ EntryId 2; EntryId 3 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> Task.FromResult allIds),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult worstIds)
            )

        let! result = resolveEntrySelector env userId (WorstKnown(AllUserEntries, count))

        Assert.Equal(Ok worstIds, result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Equal<UserId list>([ userId ], env.GetEntryIdsByUserIdCalls)
        Assert.Equal(1, env.GetWorstKnownEntryIdsCalls.Length)
    }

[<Fact>]
let ``WorstKnown AllUserEntries passes correct Count and KnowledgeWindowSize``() =
    task {
        let userId = UserId 1
        let count = 7
        let allIds = [ EntryId 1 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> Task.FromResult allIds),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult [])
            )

        let! _ = resolveEntrySelector env userId (WorstKnown(AllUserEntries, count))

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Equal<UserId list>([ userId ], env.GetEntryIdsByUserIdCalls)

        Assert.Equal<GetWorstKnownEntryIdsData list>(
            [ { UserId = userId
                ScopedEntryIds = allIds
                Count = count
                KnowledgeWindowSize = Limits.KnowledgeWindowSize } ],
            env.GetWorstKnownEntryIdsCalls
        )
    }

[<Fact>]
let ``WorstKnown AllUserEntries returns IDs from GetWorstKnownEntryIds``() =
    task {
        let userId = UserId 1
        let count = 5

        let allIds =
            [ EntryId 1; EntryId 2; EntryId 3 ]

        let worstIds = [ EntryId 2 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> Task.FromResult allIds),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult worstIds)
            )

        let! result = resolveEntrySelector env userId (WorstKnown(AllUserEntries, count))

        Assert.Equal(Ok worstIds, result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Equal<UserId list>([ userId ], env.GetEntryIdsByUserIdCalls)
        Assert.Equal(1, env.GetWorstKnownEntryIdsCalls.Length)
    }

[<Fact>]
let ``WorstKnown AllUserEntries calls GetWorstKnownEntryIds with empty ScopedEntryIds when user has no entries``() =
    task {
        let userId = UserId 1
        let count = 5

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> Task.FromResult []),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult [])
            )

        let! _ = resolveEntrySelector env userId (WorstKnown(AllUserEntries, count))

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Equal<UserId list>([ userId ], env.GetEntryIdsByUserIdCalls)

        Assert.Equal<GetWorstKnownEntryIdsData list>(
            [ { UserId = userId
                ScopedEntryIds = []
                Count = count
                KnowledgeWindowSize = Limits.KnowledgeWindowSize } ],
            env.GetWorstKnownEntryIdsCalls
        )
    }

[<Fact>]
let ``WorstKnown WithinVocabulary returns error when ownership fails without further calls``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10
        let count = 5

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult false),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (WorstKnown(WithinVocabulary vocabularyId, count))

        Assert.Equal(Error(SelectorError.VocabularyNotOwnedByUser vocabularyId), result)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``WorstKnown WithinVocabulary fetches vocabulary-scoped IDs and passes them as ScopedEntryIds``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10
        let count = 5
        let scopedIds = [ EntryId 1; EntryId 2 ]
        let worstIds = [ EntryId 2 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult true),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> Task.FromResult scopedIds),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult worstIds)
            )

        let! result = resolveEntrySelector env userId (WorstKnown(WithinVocabulary vocabularyId, count))

        Assert.Equal(Ok worstIds, result)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)

        Assert.Equal<GetEntryIdsByVocabularyIdData list>(
            [ { VocabularyId = vocabularyId
                UserId = userId } ],
            env.GetEntryIdsByVocabularyIdCalls
        )

        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)

        Assert.Equal<GetWorstKnownEntryIdsData list>(
            [ { UserId = userId
                ScopedEntryIds = scopedIds
                Count = count
                KnowledgeWindowSize = Limits.KnowledgeWindowSize } ],
            env.GetWorstKnownEntryIdsCalls
        )
    }

[<Fact>]
let ``WorstKnown WithinVocabulary passes correct Count and KnowledgeWindowSize``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10
        let count = 8
        let scopedIds = [ EntryId 1 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult true),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> Task.FromResult scopedIds),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult [])
            )

        let! _ = resolveEntrySelector env userId (WorstKnown(WithinVocabulary vocabularyId, count))

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)

        Assert.Equal<GetEntryIdsByVocabularyIdData list>(
            [ { VocabularyId = vocabularyId
                UserId = userId } ],
            env.GetEntryIdsByVocabularyIdCalls
        )

        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)

        Assert.Equal<GetWorstKnownEntryIdsData list>(
            [ { UserId = userId
                ScopedEntryIds = scopedIds
                Count = count
                KnowledgeWindowSize = Limits.KnowledgeWindowSize } ],
            env.GetWorstKnownEntryIdsCalls
        )
    }

[<Fact>]
let ``WorstKnown WithinVocabulary returns IDs from GetWorstKnownEntryIds``() =
    task {
        let userId = UserId 1
        let vocabularyId = VocabularyId 10
        let count = 5

        let scopedIds =
            [ EntryId 1; EntryId 2; EntryId 3 ]

        let worstIds = [ EntryId 3 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> Task.FromResult true),
                getCollectionById = (fun _ -> failwith "Should not be called"),
                getEntryIdsByVocabularyId = (fun _ -> Task.FromResult scopedIds),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult worstIds)
            )

        let! result = resolveEntrySelector env userId (WorstKnown(WithinVocabulary vocabularyId, count))

        Assert.Equal(Ok worstIds, result)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = vocabularyId
                 UserId = userId }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.GetCollectionByIdCalls)

        Assert.Equal<GetEntryIdsByVocabularyIdData list>(
            [ { VocabularyId = vocabularyId
                UserId = userId } ],
            env.GetEntryIdsByVocabularyIdCalls
        )

        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Equal(1, env.GetWorstKnownEntryIdsCalls.Length)
    }

[<Fact>]
let ``WorstKnown WithinCollection returns error when collection is None without further calls``() =
    task {
        let userId = UserId 1
        let collectionId = CollectionId 5
        let count = 5

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult None),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (WorstKnown(WithinCollection collectionId, count))

        Assert.Equal(Error(SelectorError.CollectionNotOwnedByUser collectionId), result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``WorstKnown WithinCollection returns error when collection belongs to another user without further calls``() =
    task {
        let userId = UserId 1
        let otherUserId = UserId 2
        let collectionId = CollectionId 5
        let count = 5

        let collection =
            makeCollection 5 (UserId.value otherUserId)

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> failwith "Should not be called"),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> failwith "Should not be called")
            )

        let! result = resolveEntrySelector env userId (WorstKnown(WithinCollection collectionId, count))

        Assert.Equal(Error(SelectorError.CollectionNotOwnedByUser collectionId), result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)
        Assert.Empty(env.GetEntryIdsByCollectionIdCalls)
        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Empty(env.GetWorstKnownEntryIdsCalls)
    }

[<Fact>]
let ``WorstKnown WithinCollection fetches collection-scoped IDs and passes them as ScopedEntryIds``() =
    task {
        let userId = UserId 1
        let collectionId = CollectionId 5
        let count = 5
        let collection = makeCollection 5 1
        let scopedIds = [ EntryId 10; EntryId 20 ]
        let worstIds = [ EntryId 10 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> Task.FromResult scopedIds),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult worstIds)
            )

        let! result = resolveEntrySelector env userId (WorstKnown(WithinCollection collectionId, count))

        Assert.Equal(Ok worstIds, result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)

        Assert.Equal<GetEntryIdsByCollectionIdData list>(
            [ { CollectionId = collectionId
                UserId = userId } ],
            env.GetEntryIdsByCollectionIdCalls
        )

        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)

        Assert.Equal<GetWorstKnownEntryIdsData list>(
            [ { UserId = userId
                ScopedEntryIds = scopedIds
                Count = count
                KnowledgeWindowSize = Limits.KnowledgeWindowSize } ],
            env.GetWorstKnownEntryIdsCalls
        )
    }

[<Fact>]
let ``WorstKnown WithinCollection passes correct Count and KnowledgeWindowSize``() =
    task {
        let userId = UserId 1
        let collectionId = CollectionId 5
        let count = 3
        let collection = makeCollection 5 1
        let scopedIds = [ EntryId 1 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> Task.FromResult scopedIds),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult [])
            )

        let! _ = resolveEntrySelector env userId (WorstKnown(WithinCollection collectionId, count))

        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)

        Assert.Equal<GetEntryIdsByCollectionIdData list>(
            [ { CollectionId = collectionId
                UserId = userId } ],
            env.GetEntryIdsByCollectionIdCalls
        )

        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)

        Assert.Equal<GetWorstKnownEntryIdsData list>(
            [ { UserId = userId
                ScopedEntryIds = scopedIds
                Count = count
                KnowledgeWindowSize = Limits.KnowledgeWindowSize } ],
            env.GetWorstKnownEntryIdsCalls
        )
    }

[<Fact>]
let ``WorstKnown WithinCollection returns IDs from GetWorstKnownEntryIds``() =
    task {
        let userId = UserId 1
        let collectionId = CollectionId 5
        let count = 5
        let collection = makeCollection 5 1

        let scopedIds =
            [ EntryId 10; EntryId 20; EntryId 30 ]

        let worstIds = [ EntryId 30 ]

        let env =
            TestEnv(
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                getEntryIdsByVocabularyId = (fun _ -> failwith "Should not be called"),
                getEntryIdsByCollectionId = (fun _ -> Task.FromResult scopedIds),
                getOwnedEntryIds = (fun _ -> failwith "Should not be called"),
                getEntryIdsByUserId = (fun _ -> failwith "Should not be called"),
                getWorstKnownEntryIds = (fun _ -> Task.FromResult worstIds)
            )

        let! result = resolveEntrySelector env userId (WorstKnown(WithinCollection collectionId, count))

        Assert.Equal(Ok worstIds, result)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Equal<CollectionId list>([ collectionId ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetEntryIdsByVocabularyIdCalls)

        Assert.Equal<GetEntryIdsByCollectionIdData list>(
            [ { CollectionId = collectionId
                UserId = userId } ],
            env.GetEntryIdsByCollectionIdCalls
        )

        Assert.Empty(env.GetOwnedEntryIdsCalls)
        Assert.Empty(env.GetEntryIdsByUserIdCalls)
        Assert.Equal(1, env.GetWorstKnownEntryIdsCalls.Length)
    }
