namespace Wordfolio.Api.Domain.Entries

open Wordfolio.Api.Domain

type EntryError =
    | EntryNotFound of EntryId
    | EntryTextRequired
    | EntryTextTooLong of maxLength: int
    | VocabularyNotFoundOrAccessDenied of VocabularyId
    | DuplicateEntry of existingEntry: Entry
    | NoDefinitionsOrTranslations
    | TooManyExamples of maxCount: int
    | ExampleTextTooLong of maxLength: int
