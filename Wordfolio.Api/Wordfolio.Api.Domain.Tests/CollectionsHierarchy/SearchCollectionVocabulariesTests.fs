module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.SearchCollectionVocabulariesTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv(searchCollectionVocabularies: SearchCollectionVocabulariesData -> Task<VocabularyWithEntryCount list>) =
    let searchCollectionVocabulariesCalls =
        ResizeArray<SearchCollectionVocabulariesData>()

    member _.SearchCollectionVocabulariesCalls =
        searchCollectionVocabulariesCalls
        |> Seq.toList

    interface ISearchCollectionVocabularies with
        member _.SearchCollectionVocabularies(data) =
            searchCollectionVocabulariesCalls.Add(data)
            searchCollectionVocabularies(data)

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
        let query: SearchCollectionVocabulariesQuery =
            { Search = Some "Test"
              SortBy = VocabularySortBy.UpdatedAt
              SortDirection = SortDirection.Desc }

        let vocabularies =
            [ makeVocabularySummary 10 "Vocabulary 1" 3
              makeVocabularySummary 11 "Vocabulary 2" 1 ]

        let env =
            TestEnv(fun data ->
                if data.UserId <> UserId 1 then
                    failwith $"Unexpected userId: {data.UserId}"

                if data.CollectionId <> CollectionId 5 then
                    failwith $"Unexpected collectionId: {data.CollectionId}"

                if data.Query <> query then
                    failwith $"Unexpected query: {data.Query}"

                Task.FromResult(vocabularies))

        let! result =
            searchCollectionVocabularies
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  Query = query }

        let expected = Ok vocabularies

        Assert.Equal(expected, result)

        let expectedCalls =
            [ ({ UserId = UserId 1
                 CollectionId = CollectionId 5
                 Query = query }
              : SearchCollectionVocabulariesData) ]

        Assert.Equal<SearchCollectionVocabulariesData list>(expectedCalls, env.SearchCollectionVocabulariesCalls)
    }
