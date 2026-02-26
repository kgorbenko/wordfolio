module Wordfolio.Api.Domain.Tests.GetOrCreateDefaultVocabularyTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Operations

type TestEnv
    (
        getDefaultVocabulary: UserId -> Task<Vocabulary option>,
        createDefaultVocabulary: CreateDefaultVocabularyParameters -> Task<VocabularyId>,
        getDefaultCollection: UserId -> Task<Collection option>,
        createDefaultCollection: CreateDefaultCollectionParameters -> Task<CollectionId>
    ) =
    let getDefaultVocabularyCalls =
        ResizeArray<UserId>()

    let createDefaultVocabularyCalls =
        ResizeArray<CreateDefaultVocabularyParameters>()

    let getDefaultCollectionCalls =
        ResizeArray<UserId>()

    let createDefaultCollectionCalls =
        ResizeArray<CreateDefaultCollectionParameters>()

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
let ``returns existing default vocabulary id when it exists``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingVocabulary =
            makeVocabulary 1 1 now

        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some existingVocabulary)),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result = getOrCreateDefaultVocabulary env (UserId 1) now

        Assert.Equal(VocabularyId 1, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultVocabularyCalls)
        Assert.Empty(env.CreateDefaultVocabularyCalls)
        Assert.Empty(env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
    }

[<Fact>]
let ``creates vocabulary when collection exists but vocabulary does not``() =
    task {
        let now = DateTimeOffset.UtcNow
        let collection = makeCollection 1 1

        let expectedVocabularyParams: CreateDefaultVocabularyParameters =
            { CollectionId = CollectionId 1
              Name = DefaultVocabularyName
              Description = None
              CreatedAt = now }

        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(None)),
                createDefaultVocabulary = (fun _ -> Task.FromResult(VocabularyId 1)),
                getDefaultCollection = (fun _ -> Task.FromResult(Some collection)),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result = getOrCreateDefaultVocabulary env (UserId 1) now

        Assert.Equal(VocabularyId 1, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultVocabularyCalls)

        Assert.Equal<CreateDefaultVocabularyParameters list>(
            [ expectedVocabularyParams ],
            env.CreateDefaultVocabularyCalls
        )

        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultCollectionCalls)
        Assert.Empty(env.CreateDefaultCollectionCalls)
    }

[<Fact>]
let ``creates both collection and vocabulary when neither exists``() =
    task {
        let now = DateTimeOffset.UtcNow

        let expectedCollectionParams: CreateDefaultCollectionParameters =
            { UserId = UserId 1
              Name = SystemCollectionName
              Description = None
              CreatedAt = now }

        let expectedVocabularyParams: CreateDefaultVocabularyParameters =
            { CollectionId = CollectionId 1
              Name = DefaultVocabularyName
              Description = None
              CreatedAt = now }

        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(None)),
                createDefaultVocabulary = (fun _ -> Task.FromResult(VocabularyId 1)),
                getDefaultCollection = (fun _ -> Task.FromResult(None)),
                createDefaultCollection = (fun _ -> Task.FromResult(CollectionId 1))
            )

        let! result = getOrCreateDefaultVocabulary env (UserId 1) now

        Assert.Equal(VocabularyId 1, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultVocabularyCalls)

        Assert.Equal<CreateDefaultVocabularyParameters list>(
            [ expectedVocabularyParams ],
            env.CreateDefaultVocabularyCalls
        )

        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultCollectionCalls)

        Assert.Equal<CreateDefaultCollectionParameters list>(
            [ expectedCollectionParams ],
            env.CreateDefaultCollectionCalls
        )
    }
