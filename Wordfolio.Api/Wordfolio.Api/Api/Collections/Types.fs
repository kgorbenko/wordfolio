namespace Wordfolio.Api.Api.Collections

open System

type CollectionResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CreateCollectionRequest =
    { Name: string
      Description: string option }

type UpdateCollectionRequest =
    { Name: string
      Description: string option }
