module Wordfolio.Api.Domain.Tests.Entries.GetDraftsTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.Operations
open Wordfolio.Api.Domain.Shared

type TestEnv
    (getDefaultVocabulary: UserId -> Task<Vocabulary option>, getEntriesHierarchy: VocabularyId -> Task<Entry list>) =
    let getDefaultVocabularyCalls =
        ResizeArray<UserId>()

    let getEntriesHierarchyCalls =
        ResizeArray<VocabularyId>()

    member _.GetDefaultVocabularyCalls =
        getDefaultVocabularyCalls |> Seq.toList

    member _.GetEntriesHierarchyCalls =
        getEntriesHierarchyCalls |> Seq.toList

    interface IGetDefaultVocabulary with
        member _.GetDefaultVocabulary(userId) =
            getDefaultVocabularyCalls.Add(userId)
            getDefaultVocabulary userId

    interface IGetEntriesHierarchy with
        member _.GetEntriesHierarchy(vocabularyId) =
            getEntriesHierarchyCalls.Add(vocabularyId)
            getEntriesHierarchy vocabularyId

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let private userId = UserId 1

let private vocabulary: Vocabulary =
    { Id = VocabularyId 10
      CollectionId = CollectionId 1
      Name = "[Default]"
      Description = None
      CreatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
      UpdatedAt = None }

let private entry: Entry =
    { Id = EntryId 100
      VocabularyId = VocabularyId 10
      EntryText = "serendipity"
      CreatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
      UpdatedAt = None
      Definitions =
        [ { Id = DefinitionId 1
            DefinitionText = "happy accident"
            Source = DefinitionSource.Api
            DisplayOrder = 0
            Examples = [] } ]
      Translations =
        [ { Id = TranslationId 1
            TranslationText = "счастливая случайность"
            Source = TranslationSource.Manual
            DisplayOrder = 0
            Examples = [] } ] }

[<Fact>]
let ``returns None when no default vocabulary exists``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult None),
                getEntriesHierarchy = (fun _ -> Task.FromResult [])
            )

        let! result = getDrafts env userId

        Assert.Equal(Ok None, result)
    }

[<Fact>]
let ``does not call getEntriesHierarchy when no default vocabulary exists``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult None),
                getEntriesHierarchy = (fun _ -> Task.FromResult [])
            )

        let! _ = getDrafts env userId

        Assert.Empty(env.GetEntriesHierarchyCalls)
    }

[<Fact>]
let ``calls getDefaultVocabulary with correct userId``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult None),
                getEntriesHierarchy = (fun _ -> Task.FromResult [])
            )

        let! _ = getDrafts env userId

        Assert.Equal<UserId list>([ userId ], env.GetDefaultVocabularyCalls)
    }

[<Fact>]
let ``returns DraftsData with empty entries when vocabulary has no entries``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some vocabulary)),
                getEntriesHierarchy = (fun _ -> Task.FromResult [])
            )

        let! result = getDrafts env userId

        let expected: DraftsData =
            { Vocabulary = vocabulary
              Entries = [] }

        Assert.Equal(Ok(Some expected), result)
    }

[<Fact>]
let ``returns DraftsData with entries when vocabulary has entries``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some vocabulary)),
                getEntriesHierarchy = (fun _ -> Task.FromResult [ entry ])
            )

        let! result = getDrafts env userId

        let expected: DraftsData =
            { Vocabulary = vocabulary
              Entries = [ entry ] }

        Assert.Equal(Ok(Some expected), result)
    }

[<Fact>]
let ``calls getEntriesHierarchy with vocabulary id from default vocabulary``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some vocabulary)),
                getEntriesHierarchy = (fun _ -> Task.FromResult [])
            )

        let! _ = getDrafts env userId

        Assert.Equal<VocabularyId list>([ vocabulary.Id ], env.GetEntriesHierarchyCalls)
    }
