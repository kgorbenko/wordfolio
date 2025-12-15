namespace Wordfolio.Api.Domain

[<Struct>]
type UserId = | UserId of int

[<Struct>]
type CollectionId = | CollectionId of int

[<Struct>]
type VocabularyId = | VocabularyId of int

module UserId =
    let value(UserId id) = id

module CollectionId =
    let value(CollectionId id) = id

module VocabularyId =
    let value(VocabularyId id) = id
