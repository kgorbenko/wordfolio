namespace Wordfolio.Api.Domain

[<Struct>]
type UserId = | UserId of int

[<Struct>]
type CollectionId = | CollectionId of int

[<Struct>]
type VocabularyId = | VocabularyId of int

[<Struct>]
type EntryId = | EntryId of int

[<Struct>]
type DefinitionId = | DefinitionId of int

[<Struct>]
type TranslationId = | TranslationId of int

[<Struct>]
type ExampleId = | ExampleId of int

module UserId =
    let value(UserId id) = id

module CollectionId =
    let value(CollectionId id) = id

module VocabularyId =
    let value(VocabularyId id) = id

module EntryId =
    let value(EntryId id) = id

module DefinitionId =
    let value(DefinitionId id) = id

module TranslationId =
    let value(TranslationId id) = id

module ExampleId =
    let value(ExampleId id) = id
