module Wordfolio.Api.Api.Types

open System
open System.Text.Json.Serialization

type DefinitionSource =
    | Api = 0
    | Manual = 1

type TranslationSource =
    | Api = 0
    | Manual = 1

type ExampleSource =
    | Api = 0
    | Custom = 1

type ExampleRequest =
    { ExampleText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: ExampleSource }

type DefinitionRequest =
    { DefinitionText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: DefinitionSource
      Examples: ExampleRequest list }

type TranslationRequest =
    { TranslationText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: TranslationSource
      Examples: ExampleRequest list }

type UpdateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list }

type ExampleResponse =
    { Id: int
      ExampleText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: ExampleSource }

type DefinitionResponse =
    { Id: int
      DefinitionText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: DefinitionSource
      DisplayOrder: int
      Examples: ExampleResponse list }

type TranslationResponse =
    { Id: int
      TranslationText: string
      [<JsonConverter(typeof<JsonStringEnumConverter>)>]
      Source: TranslationSource
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
