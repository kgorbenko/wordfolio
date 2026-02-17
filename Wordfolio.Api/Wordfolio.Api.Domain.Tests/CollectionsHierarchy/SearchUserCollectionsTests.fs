module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.SearchUserCollectionsTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv(searchUserCollections: UserId * SearchUserCollectionsQuery -> Task<CollectionOverview list>) =
    let searchUserCollectionsCalls =
        ResizeArray<UserId * SearchUserCollectionsQuery>()

    member _.SearchUserCollectionsCalls =
        searchUserCollectionsCalls |> Seq.toList

    interface ISearchUserCollections with
        member _.SearchUserCollections(userId, query) =
            searchUserCollectionsCalls.Add((userId, query))
            searchUserCollections(userId, query)

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
            TestEnv(fun (userId, requestedQuery) ->
                if userId <> UserId 1 then
                    failwith $"Unexpected userId: {userId}"

                if requestedQuery <> query then
                    failwith $"Unexpected query: {requestedQuery}"

                Task.FromResult(collections))

        let! result = searchUserCollections env (UserId 1) query

        let expected = Ok collections

        Assert.Equal(expected, result)

        let expectedCalls = [ (UserId 1, query) ]

        Assert.Equal<(UserId * SearchUserCollectionsQuery) list>(expectedCalls, env.SearchUserCollectionsCalls)
    }
