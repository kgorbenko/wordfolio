module Wordfolio.Api.Domain.Tests.Entries.GetByIdTests

open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.Operations

type TestEnv(getEntryById: EntryId -> Task<Entry option>, hasVocabularyAccess: VocabularyId * UserId -> Task<bool>) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessCalls =
        ResizeArray<VocabularyId * UserId>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.HasVocabularyAccessCalls =
        hasVocabularyAccessCalls |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(vocabularyId, userId) =
            hasVocabularyAccessCalls.Add(vocabularyId, userId)
            hasVocabularyAccess(vocabularyId, userId)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeEntry id vocabularyId entryText definitions translations =
    { Id = EntryId id
      VocabularyId = VocabularyId vocabularyId
      EntryText = entryText
      CreatedAt = System.DateTimeOffset.UtcNow
      UpdatedAt = None
      Definitions = definitions
      Translations = translations }

let makeDefinition id text source displayOrder examples =
    { Id = DefinitionId id
      DefinitionText = text
      Source = source
      DisplayOrder = displayOrder
      Examples = examples }

let makeTranslation id text source displayOrder examples =
    { Id = TranslationId id
      TranslationText = text
      Source = source
      DisplayOrder = displayOrder
      Examples = examples }

let makeExample id text source =
    { Id = ExampleId id
      ExampleText = text
      Source = source }

[<Fact>]
let ``returns entry when it exists and user has access``() =
    task {
        let example =
            makeExample 1 "example" ExampleSource.Custom

        let definition =
            makeDefinition 1 "definition" DefinitionSource.Manual 0 [ example ]

        let translation =
            makeTranslation 1 "translation" TranslationSource.Manual 0 [ example ]

        let entry =
            makeEntry 1 10 "word" [ definition ] [ translation ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result = getById env (UserId 7) (EntryId 1)

        Assert.Equal(Ok entry, result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)

        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 10, UserId 7 ], env.HasVocabularyAccessCalls)
    }

[<Fact>]
let ``returns EntryNotFound when entry does not exist``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccess = (fun _ -> failwith "Should not be called")
            )

        let! result = getById env (UserId 1) (EntryId 44)

        Assert.Equal(Error(EntryNotFound(EntryId 44)), result)
        Assert.Equal<EntryId list>([ EntryId 44 ], env.GetEntryByIdCalls)
        Assert.Empty(env.HasVocabularyAccessCalls)
    }

[<Fact>]
let ``returns EntryNotFound when user has no access``() =
    task {
        let entry = makeEntry 1 10 "word" [] []

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccess = (fun _ -> Task.FromResult(false))
            )

        let! result = getById env (UserId 2) (EntryId 1)

        Assert.Equal(Error(EntryNotFound(EntryId 1)), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)

        Assert.Equal<(VocabularyId * UserId) list>([ VocabularyId 10, UserId 2 ], env.HasVocabularyAccessCalls)
    }
