module Wordfolio.Api.Api.CollectionsHierarchy.Types

open System

type CollectionSortByRequest =
    | Name = 0
    | CreatedAt = 1
    | UpdatedAt = 2
    | VocabularyCount = 3

type VocabularySummarySortByRequest =
    | Name = 0
    | CreatedAt = 1
    | UpdatedAt = 2
    | EntryCount = 3

type SortDirectionRequest =
    | Asc = 0
    | Desc = 1

type VocabularyWithEntryCountHierarchyResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int }

type CollectionWithVocabulariesHierarchyResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularyWithEntryCountHierarchyResponse list }

type CollectionOverviewResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      VocabularyCount: int }

type VocabularySummaryResponse = VocabularyWithEntryCountHierarchyResponse
type CollectionSummaryResponse = CollectionOverviewResponse

type CollectionsHierarchyResponse =
    { Collections: CollectionWithVocabulariesHierarchyResponse list
      DefaultVocabulary: VocabularyWithEntryCountHierarchyResponse option }
