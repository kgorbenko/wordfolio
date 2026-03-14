module Wordfolio.Api.Api.CollectionsHierarchy.Types

open System

type VocabularyWithEntryCountResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int }

type CollectionWithVocabulariesResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularyWithEntryCountResponse list }

type CollectionWithVocabularyCountResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      VocabularyCount: int }

type CollectionsHierarchyResultResponse =
    { Collections: CollectionWithVocabulariesResponse list
      DefaultVocabulary: VocabularyWithEntryCountResponse option }
