namespace Wordfolio.Api.Domain

open System

type UserId = | UserId of int

type CollectionId = | CollectionId of int

type VocabularyId = | VocabularyId of int

type EntryId = | EntryId of int

type DefinitionId = | DefinitionId of int

type TranslationId = | TranslationId of int

type ExampleId = | ExampleId of int

type ExerciseSessionId = | ExerciseSessionId of int

type ExerciseAttemptId = | ExerciseAttemptId of int

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

module ExerciseSessionId =
    let value(ExerciseSessionId id) = id

module ExerciseAttemptId =
    let value(ExerciseAttemptId id) = id

type Collection =
    { Id: CollectionId
      UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type Vocabulary =
    { Id: VocabularyId
      CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type DefinitionSource =
    | Api
    | Manual

type TranslationSource =
    | Api
    | Manual

type ExampleSource =
    | Api
    | Custom

type Example =
    { Id: ExampleId
      ExampleText: string
      Source: ExampleSource }

type Definition =
    { Id: DefinitionId
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int
      Examples: Example list }

type Translation =
    { Id: TranslationId
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int
      Examples: Example list }

type Entry =
    { Id: EntryId
      VocabularyId: VocabularyId
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      Definitions: Definition list
      Translations: Translation list }
