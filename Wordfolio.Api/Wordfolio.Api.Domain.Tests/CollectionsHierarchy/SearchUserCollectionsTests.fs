module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.SearchUserCollectionsTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv(searchUserCollections: SearchUserCollectionsData -> Task<CollectionWithVocabularyCount list>) =
    let searchUserCollectionsCalls =
        ResizeArray<SearchUserCollectionsData>()

    member _.SearchUserCollectionsCalls =
        searchUserCollectionsCalls |> Seq.toList

    interface ISearchUserCollections with
        member _.SearchUserCollections(data) =
            searchUserCollectionsCalls.Add(data)
            searchUserCollections(data)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let now =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeCollectionOverview id name vocabularyCount =
    { Id = CollectionId id
      Name = name
      Description = None
      CreatedAt = now
      UpdatedAt = None
      VocabularyCount = vocabularyCount }

[<Fact>]
let ``returns collections using full query``() =
    task {
        let query: SearchUserCollectionsQuery =
            { Search = Some "Test"
              SortBy = CollectionSortBy.UpdatedAt
              SortDirection = SortDirection.Desc }

        let collections =
            [ makeCollectionOverview 1 "Collection 1" 2
              makeCollectionOverview 2 "Collection 2" 0 ]

        let env =
            TestEnv(fun data ->
                if data.UserId <> UserId 1 then
                    failwith $"Unexpected userId: {data.UserId}"

                if data.Query <> query then
                    failwith $"Unexpected query: {data.Query}"

                Task.FromResult(collections))

        let! result = searchUserCollections env { UserId = UserId 1; Query = query }

        let expected = Ok collections

        Assert.Equal(expected, result)

        let expectedCalls =
            [ ({ UserId = UserId 1; Query = query }: SearchUserCollectionsData) ]

        Assert.Equal<SearchUserCollectionsData list>(expectedCalls, env.SearchUserCollectionsCalls)
    }
