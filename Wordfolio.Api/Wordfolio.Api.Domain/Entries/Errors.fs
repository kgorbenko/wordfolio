namespace Wordfolio.Api.Domain.Entries

open Wordfolio.Api.Domain

[<RequireQualifiedAccess>]
type GetEntriesByVocabularyIdError = | VocabularyNotFoundOrAccessDenied of VocabularyId

[<RequireQualifiedAccess>]
type CreateEntryError =
    | VocabularyNotFoundOrAccessDenied of VocabularyId
    | DuplicateEntry of existingEntry: Entry
    | EntryTextRequired
    | EntryTextTooLong of maxLength: int
    | NoDefinitionsOrTranslations
    | TooManyExamples of maxCount: int
    | ExampleTextTooLong of maxLength: int

[<RequireQualifiedAccess>]
type GetEntryByIdError =
    | EntryNotFound of EntryId
    | VocabularyNotFoundOrAccessDenied of VocabularyId

[<RequireQualifiedAccess>]
type UpdateEntryError =
    | EntryNotFound of EntryId
    | VocabularyNotFoundOrAccessDenied of VocabularyId
    | EntryTextRequired
    | EntryTextTooLong of maxLength: int
    | NoDefinitionsOrTranslations
    | TooManyExamples of maxCount: int
    | ExampleTextTooLong of maxLength: int

[<RequireQualifiedAccess>]
type DeleteEntryError =
    | EntryNotFound of EntryId
    | VocabularyNotFoundOrAccessDenied of VocabularyId

[<RequireQualifiedAccess>]
type MoveEntryError =
    | EntryNotFound of EntryId
    | VocabularyNotFoundOrAccessDenied of VocabularyId

[<RequireQualifiedAccess>]
type CreateDraftEntryError =
    | DuplicateEntry of existingEntry: Entry
    | EntryTextRequired
    | EntryTextTooLong of maxLength: int
    | NoDefinitionsOrTranslations
    | TooManyExamples of maxCount: int
    | ExampleTextTooLong of maxLength: int

[<RequireQualifiedAccess>]
type GetDraftEntryByIdError = | EntryNotFound of EntryId

[<RequireQualifiedAccess>]
type GetDraftEntriesByVocabularyIdError = | VocabularyNotFoundOrAccessDenied of VocabularyId

[<RequireQualifiedAccess>]
type UpdateDraftEntryError =
    | EntryNotFound of EntryId
    | EntryTextRequired
    | EntryTextTooLong of maxLength: int
    | NoDefinitionsOrTranslations
    | TooManyExamples of maxCount: int
    | ExampleTextTooLong of maxLength: int

[<RequireQualifiedAccess>]
type MoveDraftEntryError =
    | EntryNotFound of EntryId
    | VocabularyNotFoundOrAccessDenied of VocabularyId

[<RequireQualifiedAccess>]
type DeleteDraftEntryError = | EntryNotFound of EntryId
