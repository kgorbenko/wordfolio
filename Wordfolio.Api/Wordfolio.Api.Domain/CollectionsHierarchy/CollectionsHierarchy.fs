namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System

open Wordfolio.Api.Domain

type VocabularySummary =
    { Id: VocabularyId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int }

type CollectionSummary =
    { Id: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularySummary list }

type CollectionOverview =
    { Id: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      VocabularyCount: int }

type CollectionSortBy =
    | Name
    | CreatedAt
    | UpdatedAt
    | VocabularyCount

type SortDirection =
    | Asc
    | Desc

type SearchUserCollectionsQuery =
    { Search: string option
      SortBy: CollectionSortBy
      SortDirection: SortDirection }

type CollectionsHierarchyResult =
    { Collections: CollectionSummary list
      DefaultVocabulary: VocabularySummary option }

type VocabularySummarySortBy =
    | Name
    | CreatedAt
    | UpdatedAt
    | EntryCount

type VocabularySummaryQuery =
    { Search: string option
      SortBy: VocabularySummarySortBy
      SortDirection: SortDirection }
