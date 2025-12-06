namespace Wordfolio.Api.Domain

type CollectionError =
    | CollectionNotFound of CollectionId
    | CollectionAccessDenied of CollectionId
    | CollectionNameRequired
    | CollectionNameTooLong of maxLength: int

type VocabularyError =
    | VocabularyNotFound of VocabularyId
    | VocabularyAccessDenied of VocabularyId
    | VocabularyNameRequired
    | VocabularyNameTooLong of maxLength: int
    | VocabularyCollectionNotFound of CollectionId
