namespace Wordfolio.Api.Domain.Collections

open Wordfolio.Api.Domain

[<RequireQualifiedAccess>]
type GetCollectionByIdError =
    | CollectionNotFound of CollectionId
    | CollectionAccessDenied of CollectionId

[<RequireQualifiedAccess>]
type CreateCollectionError =
    | CollectionNameRequired
    | CollectionNameTooLong of maxLength: int

[<RequireQualifiedAccess>]
type UpdateCollectionError =
    | CollectionNotFound of CollectionId
    | CollectionAccessDenied of CollectionId
    | CollectionNameRequired
    | CollectionNameTooLong of maxLength: int

[<RequireQualifiedAccess>]
type DeleteCollectionError =
    | CollectionNotFound of CollectionId
    | CollectionAccessDenied of CollectionId
