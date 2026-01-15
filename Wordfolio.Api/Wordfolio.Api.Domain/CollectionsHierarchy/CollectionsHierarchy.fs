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

type CollectionsHierarchyResult =
    { Collections: CollectionSummary list
      DefaultVocabulary: VocabularySummary option }
