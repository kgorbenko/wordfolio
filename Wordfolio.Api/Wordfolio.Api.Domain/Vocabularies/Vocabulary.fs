namespace Wordfolio.Api.Domain.Vocabularies

open System

open Wordfolio.Api.Domain

type Vocabulary = Wordfolio.Api.Domain.Vocabulary

type VocabularyDetail =
    { Id: VocabularyId
      CollectionId: CollectionId
      CollectionName: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }
