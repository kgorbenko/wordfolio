module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.GetByIdTests

open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: VocabularyId * CollectionId * UserId -> Task<bool>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<VocabularyId * CollectionId * UserId>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.HasVocabularyAccessInCollectionCalls =
        hasVocabularyAccessInCollectionCalls
        |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(vocabularyId, collectionId, userId) =
            hasVocabularyAccessInCollectionCalls.Add(vocabularyId, collectionId, userId)
            hasVocabularyAccessInCollection(vocabularyId, collectionId, userId)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeEntry id vocabularyId entryText =
    { Id = EntryId id
      VocabularyId = VocabularyId vocabularyId
      EntryText = entryText
      CreatedAt = System.DateTimeOffset.UtcNow
      UpdatedAt = None
      Definitions = []
      Translations = [] }

[<Fact>]
let ``returns entry when vocabulary is in collection and entry belongs to vocabulary``() =
    task {
        let entry = makeEntry 1 10 "word"

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true))
            )

        let! result = getById env (UserId 7) (CollectionId 5) (VocabularyId 10) (EntryId 1)

        Assert.Equal(Ok entry, result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 7 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when vocabulary is not in collection``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(false))
            )

        let! result = getById env (UserId 7) (CollectionId 5) (VocabularyId 10) (EntryId 1)

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 10)), result)
        Assert.Empty(env.GetEntryByIdCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 7 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns EntryNotFound when entry does not exist``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true))
            )

        let! result = getById env (UserId 7) (CollectionId 5) (VocabularyId 10) (EntryId 99)

        Assert.Equal(Error(EntryNotFound(EntryId 99)), result)
        Assert.Equal<EntryId list>([ EntryId 99 ], env.GetEntryByIdCalls)
    }

[<Fact>]
let ``returns EntryNotFound when entry belongs to different vocabulary``() =
    task {
        let entry = makeEntry 1 99 "word"

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true))
            )

        let! result = getById env (UserId 7) (CollectionId 5) (VocabularyId 10) (EntryId 1)

        Assert.Equal(Error(EntryNotFound(EntryId 1)), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)
    }
