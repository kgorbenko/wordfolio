module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.GetByUserIdTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv
    (
        getCollectionsWithVocabularies: UserId -> Task<CollectionSummary list>,
        getDefaultVocabularySummary: UserId -> Task<VocabularySummary option>
    ) =
    let getCollectionsWithVocabulariesCalls =
        ResizeArray<UserId>()

    let getDefaultVocabularySummaryCalls =
        ResizeArray<UserId>()

    member _.GetCollectionsWithVocabulariesCalls =
        getCollectionsWithVocabulariesCalls
        |> Seq.toList

    member _.GetDefaultVocabularySummaryCalls =
        getDefaultVocabularySummaryCalls
        |> Seq.toList

    interface IGetCollectionsWithVocabularies with
        member _.GetCollectionsWithVocabularies(userId) =
            getCollectionsWithVocabulariesCalls.Add(userId)
            getCollectionsWithVocabularies userId

    interface IGetDefaultVocabularySummary with
        member _.GetDefaultVocabularySummary(userId) =
            getDefaultVocabularySummaryCalls.Add(userId)
            getDefaultVocabularySummary userId

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let now =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeVocabularySummary vocabularyId name entryCount =
    { Id = VocabularyId vocabularyId
      Name = name
      Description = None
      CreatedAt = now
      UpdatedAt = None
      EntryCount = entryCount }

let makeCollectionSummary id name vocabularies =
    { Id = CollectionId id
      Name = name
      Description = None
      CreatedAt = now
      UpdatedAt = None
      Vocabularies = vocabularies }

[<Fact>]
let ``returns collections with vocabularies for user``() =
    task {
        let vocab1 =
            makeVocabularySummary 10 "Vocabulary 1" 5

        let vocab2 =
            makeVocabularySummary 11 "Vocabulary 2" 3

        let collections =
            [ makeCollectionSummary 1 "Collection 1" [ vocab1; vocab2 ]
              makeCollectionSummary 2 "Collection 2" [] ]

        let env =
            TestEnv(
                (fun userId ->
                    if userId <> UserId 1 then
                        failwith $"Unexpected userId: {userId}"

                    Task.FromResult(collections)),
                (fun _ -> Task.FromResult(None))
            )

        let! result = getByUserId env (UserId 1)

        let expected: CollectionsHierarchyResult =
            { Collections = collections
              DefaultVocabulary = None }

        Assert.Equal(Ok expected, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetCollectionsWithVocabulariesCalls)
    }

[<Fact>]
let ``returns empty list when user has no collections``() =
    task {
        let env =
            TestEnv(
                (fun userId ->
                    if userId <> UserId 1 then
                        failwith $"Unexpected userId: {userId}"

                    Task.FromResult([])),
                (fun _ -> Task.FromResult(None))
            )

        let! result = getByUserId env (UserId 1)

        let expected: CollectionsHierarchyResult =
            { Collections = []
              DefaultVocabulary = None }

        Assert.Equal(Ok expected, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetCollectionsWithVocabulariesCalls)
    }

[<Fact>]
let ``returns default vocabulary when it has entries``() =
    task {
        let defaultVocab =
            makeVocabularySummary 200 "My Words" 5

        let env =
            TestEnv(
                (fun _ -> Task.FromResult([])),
                (fun userId ->
                    if userId <> UserId 1 then
                        failwith $"Unexpected userId: {userId}"

                    Task.FromResult(Some defaultVocab))
            )

        let! result = getByUserId env (UserId 1)

        let expected: CollectionsHierarchyResult =
            { Collections = []
              DefaultVocabulary = Some defaultVocab }

        Assert.Equal(Ok expected, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetDefaultVocabularySummaryCalls)
    }

[<Fact>]
let ``does not return default vocabulary when it has no entries``() =
    task {
        let defaultVocab =
            makeVocabularySummary 200 "My Words" 0

        let env =
            TestEnv(
                (fun _ -> Task.FromResult([])),
                (fun userId ->
                    if userId <> UserId 1 then
                        failwith $"Unexpected userId: {userId}"

                    Task.FromResult(Some defaultVocab))
            )

        let! result = getByUserId env (UserId 1)

        let expected: CollectionsHierarchyResult =
            { Collections = []
              DefaultVocabulary = None }

        Assert.Equal(Ok expected, result)
    }
