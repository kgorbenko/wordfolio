module Wordfolio.Api.Domain.Tests.Entries.DraftOperations.GetDraftsTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.DraftOperations

let private shouldNotBeCalled<'a, 'b> : 'a -> Task<'b> =
    fun _ -> failwith "Should not be called"

type TestEnv
    (
        getDefaultVocabulary: UserId -> Task<Vocabulary option>,
        getEntriesHierarchyByVocabularyId: VocabularyId -> Task<Entry list>
    ) =
    let getDefaultVocabularyCalls =
        ResizeArray<UserId>()

    let getEntriesHierarchyByVocabularyIdCalls =
        ResizeArray<VocabularyId>()

    member _.GetDefaultVocabularyCalls =
        getDefaultVocabularyCalls |> Seq.toList

    member _.GetEntriesHierarchyByVocabularyIdCalls =
        getEntriesHierarchyByVocabularyIdCalls
        |> Seq.toList

    interface IGetDefaultVocabulary with
        member _.GetDefaultVocabulary(userId) =
            getDefaultVocabularyCalls.Add(userId)
            getDefaultVocabulary userId

    interface IGetEntriesHierarchyByVocabularyId with
        member _.GetEntriesHierarchyByVocabularyId(vocabularyId) =
            getEntriesHierarchyByVocabularyIdCalls.Add(vocabularyId)
            getEntriesHierarchyByVocabularyId vocabularyId

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
                getEntriesHierarchyByVocabularyId = shouldNotBeCalled
            )

        let! result = getDrafts env { UserId = userId }

        Assert.Equal(Ok None, result)
        Assert.Equal<UserId list>([ userId ], env.GetDefaultVocabularyCalls)
        Assert.Empty(env.GetEntriesHierarchyByVocabularyIdCalls)
    }

[<Fact>]
let ``returns DraftsVocabularyData with empty entries when vocabulary has no entries``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some vocabulary)),
                getEntriesHierarchyByVocabularyId = (fun _ -> Task.FromResult [])
            )

        let! result = getDrafts env { UserId = userId }

        let expected: DraftsVocabularyData =
            { Vocabulary = vocabulary
              Entries = [] }

        Assert.Equal(Ok(Some expected), result)
        Assert.Equal<UserId list>([ userId ], env.GetDefaultVocabularyCalls)
        Assert.Equal<VocabularyId list>([ vocabulary.Id ], env.GetEntriesHierarchyByVocabularyIdCalls)
    }

[<Fact>]
let ``returns DraftsVocabularyData with entries when vocabulary has entries``() =
    task {
        let env =
            TestEnv(
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some vocabulary)),
                getEntriesHierarchyByVocabularyId = (fun _ -> Task.FromResult [ entry ])
            )

        let! result = getDrafts env { UserId = userId }

        let expected: DraftsVocabularyData =
            { Vocabulary = vocabulary
              Entries = [ entry ] }

        Assert.Equal(Ok(Some expected), result)
        Assert.Equal<UserId list>([ userId ], env.GetDefaultVocabularyCalls)
        Assert.Equal<VocabularyId list>([ vocabulary.Id ], env.GetEntriesHierarchyByVocabularyIdCalls)
    }
