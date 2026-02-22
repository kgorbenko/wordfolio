module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.GetByVocabularyIdTests

open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations

type TestEnv
    (
        getEntriesHierarchyByVocabularyId: VocabularyId -> Task<Entry list>,
        hasVocabularyAccessInCollection: VocabularyId * CollectionId * UserId -> Task<bool>
    ) =
    let getEntriesHierarchyByVocabularyIdCalls =
        ResizeArray<VocabularyId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<VocabularyId * CollectionId * UserId>()

    member _.GetEntriesHierarchyByVocabularyIdCalls =
        getEntriesHierarchyByVocabularyIdCalls
        |> Seq.toList

    member _.HasVocabularyAccessInCollectionCalls =
        hasVocabularyAccessInCollectionCalls
        |> Seq.toList

    interface IGetEntriesHierarchyByVocabularyId with
        member _.GetEntriesHierarchyByVocabularyId(vocabularyId) =
            getEntriesHierarchyByVocabularyIdCalls.Add(vocabularyId)
            getEntriesHierarchyByVocabularyId vocabularyId

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(vocabularyId, collectionId, userId) =
            hasVocabularyAccessInCollectionCalls.Add(vocabularyId, collectionId, userId)
            hasVocabularyAccessInCollection(vocabularyId, collectionId, userId)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeEntry id vocabularyId text =
    { Id = EntryId id
      VocabularyId = VocabularyId vocabularyId
      EntryText = text
      CreatedAt = System.DateTimeOffset.UtcNow
      UpdatedAt = None
      Definitions = []
      Translations = [] }

[<Fact>]
let ``returns entries when vocabulary is in collection``() =
    task {
        let entries =
            [ makeEntry 1 9 "one"; makeEntry 2 9 "two" ]

        let env =
            TestEnv(
                getEntriesHierarchyByVocabularyId = (fun _ -> Task.FromResult(entries)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true))
            )

        let! result = getByVocabularyId env (UserId 1) (CollectionId 5) (VocabularyId 9)

        Assert.Equal(Ok entries, result)
        Assert.Equal<VocabularyId list>([ VocabularyId 9 ], env.GetEntriesHierarchyByVocabularyIdCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 9, CollectionId 5, UserId 1 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns empty list when vocabulary has no entries``() =
    task {
        let env =
            TestEnv(
                getEntriesHierarchyByVocabularyId = (fun _ -> Task.FromResult([])),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true))
            )

        let! result = getByVocabularyId env (UserId 1) (CollectionId 5) (VocabularyId 9)

        Assert.Equal(Ok [], result)
        Assert.Equal<VocabularyId list>([ VocabularyId 9 ], env.GetEntriesHierarchyByVocabularyIdCalls)
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when vocabulary is not in collection``() =
    task {
        let env =
            TestEnv(
                getEntriesHierarchyByVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(false))
            )

        let! result = getByVocabularyId env (UserId 1) (CollectionId 5) (VocabularyId 9)

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 9)), result)
        Assert.Empty(env.GetEntriesHierarchyByVocabularyIdCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 9, CollectionId 5, UserId 1 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }
