module Wordfolio.Api.Api.Vocabularies.Types

open System

type VocabularyResponse =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type VocabularyDetailResponse =
    { Id: int
      CollectionId: int
      CollectionName: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateVocabularyRequest =
    { Name: string
      Description: string option }

type UpdateVocabularyRequest =
    { Name: string
      Description: string option }
