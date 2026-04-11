namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System

open Wordfolio.Api.Domain

type VocabularyWithEntryCount =
    { Id: VocabularyId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      EntryCount: int }

type CollectionWithVocabularies =
    { Id: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      Vocabularies: VocabularyWithEntryCount list }

type CollectionWithVocabularyCount =
    { Id: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      VocabularyCount: int }

type CollectionsHierarchyResult =
    { Collections: CollectionWithVocabularies list
      DefaultVocabulary: VocabularyWithEntryCount option }
