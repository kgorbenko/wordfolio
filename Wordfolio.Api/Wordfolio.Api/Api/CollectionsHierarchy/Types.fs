module Wordfolio.Api.Api.CollectionsHierarchy.Types

open System

type VocabularyWithEntryCountResponse =
    { Id: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      EntryCount: int }

type CollectionWithVocabulariesResponse =
    { Id: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      Vocabularies: VocabularyWithEntryCountResponse list }

type CollectionWithVocabularyCountResponse =
    { Id: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      VocabularyCount: int }

type CollectionsHierarchyResultResponse =
    { Collections: CollectionWithVocabulariesResponse list
      DefaultVocabulary: VocabularyWithEntryCountResponse option }
