namespace Wordfolio.Api.Domain.Exercises

open Wordfolio.Api.Domain

[<RequireQualifiedAccess>]
type SelectorError =
    | VocabularyNotOwnedByUser of VocabularyId
    | CollectionNotOwnedByUser of CollectionId
    | EntryNotOwnedByUser of EntryId list

[<RequireQualifiedAccess>]
type CreateSessionError =
    | SelectorFailed of SelectorError
    | NoEntriesResolved

[<RequireQualifiedAccess>]
type GetSessionError = | NotFound

[<RequireQualifiedAccess>]
type SubmitAttemptError =
    | SessionNotFound
    | EntryNotInSession of EntryId
    | ConflictingAttempt
    | EvaluateError of EvaluateError
