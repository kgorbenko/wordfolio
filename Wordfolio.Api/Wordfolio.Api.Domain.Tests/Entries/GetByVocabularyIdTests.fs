module Wordfolio.Api.Domain.Tests.Entries.GetByVocabularyIdTests

open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.Operations

type TestEnv
    (
        getEntriesByVocabularyId: VocabularyId -> Task<Entry list>,
        hasVocabularyAccess: VocabularyId * UserId -> Task<bool>
    ) =
    let getEntriesByVocabularyIdCalls =
        ResizeArray<VocabularyId>()

    let hasVocabularyAccessCalls =
        ResizeArray<VocabularyId * UserId>()

    member _.GetEntriesByVocabularyIdCalls =
        getEntriesByVocabularyIdCalls
        |> Seq.toList

    member _.HasVocabularyAccessCalls =
        hasVocabularyAccessCalls |> Seq.toList

    interface IGetEntriesByVocabularyId with
        member _.GetEntriesByVocabularyId(vocabularyId) =
            getEntriesByVocabularyIdCalls.Add(vocabularyId)
            getEntriesByVocabularyId vocabularyId

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(vocabularyId, userId) =
            hasVocabularyAccessCalls.Add(vocabularyId, userId)
            hasVocabularyAccess(vocabularyId, userId)

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
let ``returns empty list when vocabulary has no entries``() =
    task {
        let env =
            TestEnv(
                getEntriesByVocabularyId = (fun _ -> Task.FromResult([])),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result = getByVocabularyId env (UserId 1) (VocabularyId 9)

        Assert.Equal(Ok [], result)
        Assert.Equal<VocabularyId list>([ VocabularyId 9 ], env.GetEntriesByVocabularyIdCalls)

        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 9, UserId 1 ], env.HasVocabularyAccessCalls)
    }

[<Fact>]
let ``returns entries when vocabulary has entries``() =
    task {
        let entries =
            [ makeEntry 1 9 "one"; makeEntry 2 9 "two" ]

        let env =
            TestEnv(
                getEntriesByVocabularyId = (fun _ -> Task.FromResult(entries)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result = getByVocabularyId env (UserId 1) (VocabularyId 9)

        Assert.Equal(Ok entries, result)
        Assert.Equal<VocabularyId list>([ VocabularyId 9 ], env.GetEntriesByVocabularyIdCalls)

        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 9, UserId 1 ], env.HasVocabularyAccessCalls)
    }

[<Fact>]
let ``returns error when user has no access``() =
    task {
        let env =
            TestEnv(
                getEntriesByVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccess = (fun _ -> Task.FromResult(false))
            )

        let! result = getByVocabularyId env (UserId 1) (VocabularyId 9)

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 9)), result)
        Assert.Empty(env.GetEntriesByVocabularyIdCalls)

        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 9, UserId 1 ], env.HasVocabularyAccessCalls)
    }
