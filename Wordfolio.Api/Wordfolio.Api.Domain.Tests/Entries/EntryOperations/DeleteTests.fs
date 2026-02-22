module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.DeleteTests

open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: VocabularyId * CollectionId * UserId -> Task<bool>,
        deleteEntry: EntryId -> Task<int>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<VocabularyId * CollectionId * UserId>()

    let deleteEntryCalls =
        ResizeArray<EntryId>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.HasVocabularyAccessInCollectionCalls =
        hasVocabularyAccessInCollectionCalls
        |> Seq.toList

    member _.DeleteEntryCalls =
        deleteEntryCalls |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(vocabularyId, collectionId, userId) =
            hasVocabularyAccessInCollectionCalls.Add(vocabularyId, collectionId, userId)
            hasVocabularyAccessInCollection(vocabularyId, collectionId, userId)

    interface IDeleteEntry with
        member _.DeleteEntry(entryId) =
            deleteEntryCalls.Add(entryId)
            deleteEntry entryId

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
let ``deletes entry when vocabulary is in collection and entry belongs to vocabulary``() =
    task {
        let entry = makeEntry 1 10 "word"

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                deleteEntry = (fun _ -> Task.FromResult(1))
            )

        let! result = delete env (UserId 1) (CollectionId 5) (VocabularyId 10) (EntryId 1)

        Assert.Equal(Ok(), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.DeleteEntryCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 1 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when vocabulary is not in collection``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(false)),
                deleteEntry = (fun _ -> failwith "Should not be called")
            )

        let! result = delete env (UserId 1) (CollectionId 5) (VocabularyId 10) (EntryId 1)

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 10)), result)
        Assert.Empty(env.GetEntryByIdCalls)
        Assert.Empty(env.DeleteEntryCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 1 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns EntryNotFound when entry does not exist``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                deleteEntry = (fun _ -> failwith "Should not be called")
            )

        let! result = delete env (UserId 1) (CollectionId 5) (VocabularyId 10) (EntryId 99)

        Assert.Equal(Error(EntryNotFound(EntryId 99)), result)
        Assert.Equal<EntryId list>([ EntryId 99 ], env.GetEntryByIdCalls)
        Assert.Empty(env.DeleteEntryCalls)
    }

[<Fact>]
let ``returns EntryNotFound when entry belongs to different vocabulary``() =
    task {
        let entry = makeEntry 1 99 "word"

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                deleteEntry = (fun _ -> failwith "Should not be called")
            )

        let! result = delete env (UserId 1) (CollectionId 5) (VocabularyId 10) (EntryId 1)

        Assert.Equal(Error(EntryNotFound(EntryId 1)), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)
        Assert.Empty(env.DeleteEntryCalls)
    }
