module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.GetByIdTests

open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: HasVocabularyAccessInCollectionData -> Task<bool>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<HasVocabularyAccessInCollectionData>()

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
        member _.HasVocabularyAccessInCollection(data) =
            hasVocabularyAccessInCollectionCalls.Add(data)
            hasVocabularyAccessInCollection data

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

        let! result =
            getById
                env
                { UserId = UserId 7
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1 }

        Assert.Equal(Ok entry, result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)

        Assert.Equal<HasVocabularyAccessInCollectionData list>(
            [ ({ VocabularyId = VocabularyId 10
                 CollectionId = CollectionId 5
                 UserId = UserId 7 }
              : HasVocabularyAccessInCollectionData) ],
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

        let! result =
            getById
                env
                { UserId = UserId 7
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1 }

        Assert.Equal(Error(GetEntryByIdError.VocabularyNotFoundOrAccessDenied(VocabularyId 10)), result)
        Assert.Empty(env.GetEntryByIdCalls)

        Assert.Equal<HasVocabularyAccessInCollectionData list>(
            [ ({ VocabularyId = VocabularyId 10
                 CollectionId = CollectionId 5
                 UserId = UserId 7 }
              : HasVocabularyAccessInCollectionData) ],
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

        let! result =
            getById
                env
                { UserId = UserId 7
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 99 }

        Assert.Equal(Error(GetEntryByIdError.EntryNotFound(EntryId 99)), result)
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

        let! result =
            getById
                env
                { UserId = UserId 7
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1 }

        Assert.Equal(Error(GetEntryByIdError.EntryNotFound(EntryId 1)), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)
    }
