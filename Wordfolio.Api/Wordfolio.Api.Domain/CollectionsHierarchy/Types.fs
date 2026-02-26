namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System

open Wordfolio.Api.Domain

type VocabularyWithEntryCount =
    { Id: VocabularyId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int }

type CollectionWithVocabularies =
    { Id: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularyWithEntryCount list }

type CollectionWithVocabularyCount =
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
    { Collections: CollectionWithVocabularies list
      DefaultVocabulary: VocabularyWithEntryCount option }

type VocabularySortBy =
    | Name
    | CreatedAt
    | UpdatedAt
    | EntryCount

type SearchCollectionVocabulariesQuery =
    { Search: string option
      SortBy: VocabularySortBy
      SortDirection: SortDirection }
