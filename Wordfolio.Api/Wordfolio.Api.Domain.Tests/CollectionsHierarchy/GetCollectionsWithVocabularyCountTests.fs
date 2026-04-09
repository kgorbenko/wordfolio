module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.GetCollectionsWithVocabularyCountTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv(getCollectionsWithVocabularyCount: UserId -> Task<CollectionWithVocabularyCount list>) =
    let getCollectionsWithVocabularyCountCalls =
        ResizeArray<UserId>()

    member _.GetCollectionsWithVocabularyCountCalls =
        getCollectionsWithVocabularyCountCalls
        |> Seq.toList

    interface IGetCollectionsWithVocabularyCount with
        member _.GetCollectionsWithVocabularyCount(userId) =
            getCollectionsWithVocabularyCountCalls.Add(userId)
            getCollectionsWithVocabularyCount userId

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let now =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeCollectionWithVocabularyCount id name vocabularyCount =
    { Id = CollectionId id
      Name = name
      Description = None
      CreatedAt = now
      UpdatedAt = now
      VocabularyCount = vocabularyCount }

[<Fact>]
let ``returns collections from capability``() =
    task {
        let collections =
            [ makeCollectionWithVocabularyCount 1 "Collection 1" 2
              makeCollectionWithVocabularyCount 2 "Collection 2" 0 ]

        let env =
            TestEnv(fun userId ->
                if userId <> UserId 1 then
                    failwith $"Unexpected userId: {userId}"

                Task.FromResult(collections))

        let! result = getCollectionsWithVocabularyCount env (UserId 1)

        Assert.Equal(Ok collections, result)

        Assert.Equal<UserId list>([ UserId 1 ], env.GetCollectionsWithVocabularyCountCalls)
    }
