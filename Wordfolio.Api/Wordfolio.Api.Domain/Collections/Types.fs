namespace Wordfolio.Api.Domain.Collections

type CollectionNameValidationError =
    | NameRequired
    | NameTooLong of maxLength: int
