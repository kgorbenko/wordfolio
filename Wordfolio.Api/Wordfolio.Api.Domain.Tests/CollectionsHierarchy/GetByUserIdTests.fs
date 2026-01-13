module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.GetByUserIdTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv(getCollectionsWithVocabularies: UserId -> Task<CollectionSummary list>) =
    let getCollectionsWithVocabulariesCalls =
        ResizeArray<UserId>()

    member _.GetCollectionsWithVocabulariesCalls =
        getCollectionsWithVocabulariesCalls
        |> Seq.toList

    interface IGetCollectionsWithVocabularies with
        member _.GetCollectionsWithVocabularies(userId) =
            getCollectionsWithVocabulariesCalls.Add(userId)
            getCollectionsWithVocabularies userId

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let now =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeVocabularySummary collectionId vocabularyId name entryCount =
    { Id = VocabularyId vocabularyId
      CollectionId = CollectionId collectionId
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
            makeVocabularySummary 1 10 "Vocabulary 1" 5

        let vocab2 =
            makeVocabularySummary 1 11 "Vocabulary 2" 3

        let collections =
            [ makeCollectionSummary 1 "Collection 1" [ vocab1; vocab2 ]
              makeCollectionSummary 2 "Collection 2" [] ]

        let env =
            TestEnv(fun userId ->
                if userId <> UserId 1 then
                    failwith $"Unexpected userId: {userId}"

                Task.FromResult(collections))

        let! result = getByUserId env (UserId 1)

        Assert.Equal(Ok collections, result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetCollectionsWithVocabulariesCalls)
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

        Assert.Equal(Ok [], result)
        Assert.Equal<UserId list>([ UserId 1 ], env.GetCollectionsWithVocabulariesCalls)
    }
