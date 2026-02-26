module Wordfolio.Api.Domain.Tests.Entries.DraftOperations.DeleteTests

open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.DraftOperations

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccess: HasVocabularyAccessData -> Task<bool>,
        deleteEntry: EntryId -> Task<int>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessCalls =
        ResizeArray<HasVocabularyAccessData>()

    let deleteEntryCalls =
        ResizeArray<EntryId>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.HasVocabularyAccessCalls =
        hasVocabularyAccessCalls |> Seq.toList

    member _.DeleteEntryCalls =
        deleteEntryCalls |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(data) =
            hasVocabularyAccessCalls.Add(data)
            hasVocabularyAccess(data)

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
let ``deletes entry when user has access``() =
    task {
        let entry = makeEntry 1 10 "word"

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true)),
                deleteEntry = (fun _ -> Task.FromResult(1))
            )

        let! result =
            delete
                env
                { UserId = UserId 1
                  EntryId = EntryId 1 }

        Assert.Equal(Ok(), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.DeleteEntryCalls)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = VocabularyId 10
                 UserId = UserId 1 }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )
    }

[<Fact>]
let ``returns EntryNotFound when entry does not exist``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccess = (fun _ -> failwith "Should not be called"),
                deleteEntry = (fun _ -> failwith "Should not be called")
            )

        let! result =
            delete
                env
                { UserId = UserId 1
                  EntryId = EntryId 2 }

        Assert.Equal(Error(DeleteDraftEntryError.EntryNotFound(EntryId 2)), result)
        Assert.Equal<EntryId list>([ EntryId 2 ], env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
        Assert.Empty(env.DeleteEntryCalls)
    }

[<Fact>]
let ``returns EntryNotFound when user has no access``() =
    task {
        let entry = makeEntry 1 10 "word"

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(false)),
                deleteEntry = (fun _ -> failwith "Should not be called")
            )

        let! result =
            delete
                env
                { UserId = UserId 3
                  EntryId = EntryId 1 }

        Assert.Equal(Error(DeleteDraftEntryError.EntryNotFound(EntryId 1)), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)

        Assert.Equal<HasVocabularyAccessData list>(
            [ ({ VocabularyId = VocabularyId 10
                 UserId = UserId 3 }
              : HasVocabularyAccessData) ],
            env.HasVocabularyAccessCalls
        )

        Assert.Empty(env.DeleteEntryCalls)
    }
