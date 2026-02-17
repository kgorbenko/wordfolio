module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.SearchCollectionVocabulariesTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv
    (searchCollectionVocabularies: UserId * CollectionId * VocabularySummaryQuery -> Task<VocabularySummary list>) =
    let searchCollectionVocabulariesCalls =
        ResizeArray<UserId * CollectionId * VocabularySummaryQuery>()

    member _.SearchCollectionVocabulariesCalls =
        searchCollectionVocabulariesCalls
        |> Seq.toList

    interface ISearchCollectionVocabularies with
        member _.SearchCollectionVocabularies(userId, collectionId, query) =
            searchCollectionVocabulariesCalls.Add((userId, collectionId, query))
            searchCollectionVocabularies(userId, collectionId, query)

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

[<Fact>]
let ``returns vocabularies using full query``() =
    task {
        let query: VocabularySummaryQuery =
            { Search = Some "Test"
              SortBy = VocabularySummarySortBy.UpdatedAt
              SortDirection = SortDirection.Desc }

        let vocabularies =
            [ makeVocabularySummary 10 "Vocabulary 1" 3
              makeVocabularySummary 11 "Vocabulary 2" 1 ]

        let env =
            TestEnv(fun (userId, collectionId, requestedQuery) ->
                if userId <> UserId 1 then
                    failwith $"Unexpected userId: {userId}"

                if collectionId <> CollectionId 5 then
                    failwith $"Unexpected collectionId: {collectionId}"

                if requestedQuery <> query then
                    failwith $"Unexpected query: {requestedQuery}"

                Task.FromResult(vocabularies))

        let! result = searchCollectionVocabularies env (UserId 1) (CollectionId 5) query

        let expected = Ok vocabularies

        Assert.Equal(expected, result)

        let expectedCalls =
            [ (UserId 1, CollectionId 5, query) ]

        Assert.Equal<(UserId * CollectionId * VocabularySummaryQuery) list>(
            expectedCalls,
            env.SearchCollectionVocabulariesCalls
        )
    }
