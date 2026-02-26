namespace Wordfolio.Api.Domain.Vocabularies

open Wordfolio.Api.Domain

[<RequireQualifiedAccess>]
type GetVocabularyByIdError =
    | VocabularyNotFound of VocabularyId
    | VocabularyAccessDenied of VocabularyId
    | VocabularyCollectionNotFound of CollectionId

[<RequireQualifiedAccess>]
type GetVocabulariesByCollectionIdError = | VocabularyCollectionNotFound of CollectionId

[<RequireQualifiedAccess>]
type CreateVocabularyError =
    | VocabularyNameRequired
    | VocabularyNameTooLong of maxLength: int
    | VocabularyCollectionNotFound of CollectionId

[<RequireQualifiedAccess>]
type UpdateVocabularyError =
    | VocabularyNotFound of VocabularyId
    | VocabularyAccessDenied of VocabularyId
    | VocabularyNameRequired
    | VocabularyNameTooLong of maxLength: int

[<RequireQualifiedAccess>]
type DeleteVocabularyError =
    | VocabularyNotFound of VocabularyId
    | VocabularyAccessDenied of VocabularyId
