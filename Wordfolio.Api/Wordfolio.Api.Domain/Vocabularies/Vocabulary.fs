namespace Wordfolio.Api.Domain.Vocabularies

open System

open Wordfolio.Api.Domain

type Vocabulary =
    { Id: VocabularyId
      CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }
