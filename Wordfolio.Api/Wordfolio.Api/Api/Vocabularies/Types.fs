module Wordfolio.Api.Api.Vocabularies.Types

open System

type VocabularyResponse =
    { Id: string
      CollectionId: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type VocabularyDetailResponse =
    { Id: string
      CollectionId: string
      CollectionName: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type CreateVocabularyRequest =
    { Name: string
      Description: string option }

type UpdateVocabularyRequest =
    { Name: string
      Description: string option }

type MoveVocabularyRequest = { CollectionId: string }
