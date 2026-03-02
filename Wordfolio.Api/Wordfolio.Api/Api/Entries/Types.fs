namespace Wordfolio.Api.Api.Entries

type DefinitionSourceDto = Wordfolio.Api.Api.DefinitionSourceDto
type TranslationSourceDto = Wordfolio.Api.Api.TranslationSourceDto
type ExampleSourceDto = Wordfolio.Api.Api.ExampleSourceDto

type ExampleRequest = Wordfolio.Api.Api.ExampleRequest
type DefinitionRequest = Wordfolio.Api.Api.DefinitionRequest
type TranslationRequest = Wordfolio.Api.Api.TranslationRequest

type UpdateEntryRequest = Wordfolio.Api.Api.UpdateEntryRequest
type MoveEntryRequest = Wordfolio.Api.Api.MoveEntryRequest

type ExampleResponse = Wordfolio.Api.Api.ExampleResponse
type DefinitionResponse = Wordfolio.Api.Api.DefinitionResponse
type TranslationResponse = Wordfolio.Api.Api.TranslationResponse
type EntryResponse = Wordfolio.Api.Api.EntryResponse

type CreateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }
