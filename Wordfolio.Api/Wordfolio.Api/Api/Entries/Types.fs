module Wordfolio.Api.Api.Entries.Types

open System
open System.Text.Json.Serialization

type DefinitionSourceDto =
    | Api = 0
    | Manual = 1

type TranslationSourceDto =
    | Api = 0
    | Manual = 1

type ExampleSourceDto =
    | Api = 0
    | Custom = 1

type ExampleRequest =
    { ExampleText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: ExampleSourceDto }

type DefinitionRequest =
    { DefinitionText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: DefinitionSourceDto
      Examples: ExampleRequest list }

type TranslationRequest =
    { TranslationText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: TranslationSourceDto
      Examples: ExampleRequest list }

type CreateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }

type UpdateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list }

type MoveEntryRequest = { VocabularyId: int }

type ExampleResponse =
    { Id: int
      ExampleText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: ExampleSourceDto }

type DefinitionResponse =
    { Id: int
      DefinitionText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: DefinitionSourceDto
      DisplayOrder: int
      Examples: ExampleResponse list }

type TranslationResponse =
    { Id: int
      TranslationText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: TranslationSourceDto
      DisplayOrder: int
      Examples: ExampleResponse list }

type EntryResponse =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Definitions: DefinitionResponse list
      Translations: TranslationResponse list }
