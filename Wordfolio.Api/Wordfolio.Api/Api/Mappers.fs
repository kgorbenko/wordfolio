module Wordfolio.Api.Api.Mappers

open Wordfolio.Api.Api.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries

let private toDefinitionSourceDomain(source: DefinitionSourceDto) : DefinitionSource =
    match source with
    | DefinitionSourceDto.Api -> DefinitionSource.Api
    | DefinitionSourceDto.Manual -> DefinitionSource.Manual
    | _ -> DefinitionSource.Manual

let private toTranslationSourceDomain(source: TranslationSourceDto) : TranslationSource =
    match source with
    | TranslationSourceDto.Api -> TranslationSource.Api
    | TranslationSourceDto.Manual -> TranslationSource.Manual
    | _ -> TranslationSource.Manual

let private toExampleSourceDomain(source: ExampleSourceDto) : ExampleSource =
    match source with
    | ExampleSourceDto.Api -> ExampleSource.Api
    | ExampleSourceDto.Custom -> ExampleSource.Custom
    | _ -> ExampleSource.Custom

let private toExampleSourceDto(source: ExampleSource) : ExampleSourceDto =
    match source with
    | ExampleSource.Api -> ExampleSourceDto.Api
    | ExampleSource.Custom -> ExampleSourceDto.Custom

let private toDefinitionSourceDto(source: DefinitionSource) : DefinitionSourceDto =
    match source with
    | DefinitionSource.Api -> DefinitionSourceDto.Api
    | DefinitionSource.Manual -> DefinitionSourceDto.Manual

let private toTranslationSourceDto(source: TranslationSource) : TranslationSourceDto =
    match source with
    | TranslationSource.Api -> TranslationSourceDto.Api
    | TranslationSource.Manual -> TranslationSourceDto.Manual

let toExampleInput(request: ExampleRequest) : ExampleInput =
    { ExampleText = request.ExampleText
      Source = toExampleSourceDomain request.Source }

let toDefinitionInput(request: DefinitionRequest) : DefinitionInput =
    { DefinitionText = request.DefinitionText
      Source = toDefinitionSourceDomain request.Source
      Examples =
        request.Examples
        |> List.map toExampleInput }

let toTranslationInput(request: TranslationRequest) : TranslationInput =
    { TranslationText = request.TranslationText
      Source = toTranslationSourceDomain request.Source
      Examples =
        request.Examples
        |> List.map toExampleInput }

let private toExampleResponse(example: Example) : ExampleResponse =
    { Id = ExampleId.value example.Id
      ExampleText = example.ExampleText
      Source = toExampleSourceDto example.Source }

let private toDefinitionResponse(definition: Definition) : DefinitionResponse =
    { Id = DefinitionId.value definition.Id
      DefinitionText = definition.DefinitionText
      Source = toDefinitionSourceDto definition.Source
      DisplayOrder = definition.DisplayOrder
      Examples =
        definition.Examples
        |> List.map toExampleResponse }

let private toTranslationResponse(translation: Translation) : TranslationResponse =
    { Id = TranslationId.value translation.Id
      TranslationText = translation.TranslationText
      Source = toTranslationSourceDto translation.Source
      DisplayOrder = translation.DisplayOrder
      Examples =
        translation.Examples
        |> List.map toExampleResponse }

let toEntryResponse(entry: Entry) : EntryResponse =
    { Id = EntryId.value entry.Id
      VocabularyId = VocabularyId.value entry.VocabularyId
      EntryText = entry.EntryText
      CreatedAt = entry.CreatedAt
      UpdatedAt = entry.UpdatedAt
      Definitions =
        entry.Definitions
        |> List.map toDefinitionResponse
      Translations =
        entry.Translations
        |> List.map toTranslationResponse }
