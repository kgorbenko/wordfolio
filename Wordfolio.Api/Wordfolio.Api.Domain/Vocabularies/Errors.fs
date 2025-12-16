namespace Wordfolio.Api.Domain.Vocabularies

open Wordfolio.Api.Domain

type VocabularyError =
    | VocabularyNotFound of VocabularyId
    | VocabularyAccessDenied of VocabularyId
    | VocabularyNameRequired
    | VocabularyNameTooLong of maxLength: int
    | VocabularyCollectionNotFound of CollectionId
