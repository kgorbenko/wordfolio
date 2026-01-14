namespace Wordfolio.Api.Domain.Shared

open System

open Wordfolio.Api.Domain

type Collection =
    { Id: CollectionId
      UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type Vocabulary =
    { Id: VocabularyId
      CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }
