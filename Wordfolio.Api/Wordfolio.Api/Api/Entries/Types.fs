module Wordfolio.Api.Api.Entries.Types

open Wordfolio.Api.Api.Types

type DefinitionSourceDto = Wordfolio.Api.Api.Types.DefinitionSourceDto
type TranslationSourceDto = Wordfolio.Api.Api.Types.TranslationSourceDto
type ExampleSourceDto = Wordfolio.Api.Api.Types.ExampleSourceDto

type ExampleRequest = Wordfolio.Api.Api.Types.ExampleRequest
type DefinitionRequest = Wordfolio.Api.Api.Types.DefinitionRequest
type TranslationRequest = Wordfolio.Api.Api.Types.TranslationRequest

type UpdateEntryRequest = Wordfolio.Api.Api.Types.UpdateEntryRequest
type MoveEntryRequest = Wordfolio.Api.Api.Types.MoveEntryRequest

type ExampleResponse = Wordfolio.Api.Api.Types.ExampleResponse
type DefinitionResponse = Wordfolio.Api.Api.Types.DefinitionResponse
type TranslationResponse = Wordfolio.Api.Api.Types.TranslationResponse
type EntryResponse = Wordfolio.Api.Api.Types.EntryResponse

type CreateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }
