namespace Wordfolio.Api.Domain.Collections

open System

open Wordfolio.Api.Domain

type Collection =
    { Id: CollectionId
      UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }
