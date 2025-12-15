namespace Wordfolio.Api.Domain.Collections

open Wordfolio.Api.Domain

type CollectionError =
    | CollectionNotFound of CollectionId
    | CollectionAccessDenied of CollectionId
    | CollectionNameRequired
    | CollectionNameTooLong of maxLength: int
