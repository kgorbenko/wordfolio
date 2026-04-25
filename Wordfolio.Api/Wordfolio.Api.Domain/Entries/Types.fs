namespace Wordfolio.Api.Domain.Entries

open System

open Wordfolio.Api.Domain

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
      VocabularyId: VocabularyId
      EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list
      AllowDuplicate: bool
      CreatedAt: DateTimeOffset }

type CreateDraftParameters =
    { UserId: UserId
      EntryText: string
      Definitions: DefinitionInput list
      Translations: TranslationInput list
      AllowDuplicate: bool
      CreatedAt: DateTimeOffset }

type DraftsVocabularyData =
    { Vocabulary: Vocabulary
      Entries: Entry list }
