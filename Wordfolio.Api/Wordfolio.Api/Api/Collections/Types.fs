module Wordfolio.Api.Api.Collections.Types

open System

type CollectionResponse =
    { Id: string
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type CreateCollectionRequest =
    { Name: string
      Description: string option }

type UpdateCollectionRequest =
    { Name: string
      Description: string option }
