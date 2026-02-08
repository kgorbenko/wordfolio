namespace Wordfolio.Api.Domain.Entries

open System

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Shared

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
      UpdatedAt: DateTimeOffset option
      Definitions: Definition list
      Translations: Translation list }

type ExampleInput =
    { ExampleText: string
      Source: ExampleSource }

type DefinitionInput =
    { DefinitionText: string
      Source: DefinitionSource
      Examples: ExampleInput list }

type TranslationInput =
    { TranslationText: string
      Source: TranslationSource
      Examples: ExampleInput list }

type CreateEntryParameters =
    { UserId: UserId
      VocabularyId: VocabularyId option
      EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list
      AllowDuplicate: bool
      CreatedAt: DateTimeOffset }

type DraftsVocabularyData =
    { Vocabulary: Vocabulary
      Entries: Entry list }
