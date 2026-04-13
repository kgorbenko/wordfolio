module Wordfolio.Api.Api.Mappers

open Wordfolio.Api.Api.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

module ApiTypes = Wordfolio.Api.Api.Types

let private toDefinitionSourceDomain(source: ApiTypes.DefinitionSource) : DefinitionSource =
    match source with
    | ApiTypes.DefinitionSource.Api -> DefinitionSource.Api
    | ApiTypes.DefinitionSource.Manual -> DefinitionSource.Manual
    | x -> failwith $"Unknown {nameof ApiTypes.DefinitionSource} value: {x}"

let private toTranslationSourceDomain(source: ApiTypes.TranslationSource) : TranslationSource =
    match source with
    | ApiTypes.TranslationSource.Api -> TranslationSource.Api
    | ApiTypes.TranslationSource.Manual -> TranslationSource.Manual
    | x -> failwith $"Unknown {nameof ApiTypes.TranslationSource} value: {x}"

let private toExampleSourceDomain(source: ApiTypes.ExampleSource) : ExampleSource =
    match source with
    | ApiTypes.ExampleSource.Api -> ExampleSource.Api
    | ApiTypes.ExampleSource.Custom -> ExampleSource.Custom
    | x -> failwith $"Unknown {nameof ApiTypes.ExampleSource} value: {x}"

let private toExampleSourceDto(source: ExampleSource) : ApiTypes.ExampleSource =
    match source with
    | ExampleSource.Api -> ApiTypes.ExampleSource.Api
    | ExampleSource.Custom -> ApiTypes.ExampleSource.Custom

let private toDefinitionSourceDto(source: DefinitionSource) : ApiTypes.DefinitionSource =
    match source with
    | DefinitionSource.Api -> ApiTypes.DefinitionSource.Api
    | DefinitionSource.Manual -> ApiTypes.DefinitionSource.Manual

let private toTranslationSourceDto(source: TranslationSource) : ApiTypes.TranslationSource =
    match source with
    | TranslationSource.Api -> ApiTypes.TranslationSource.Api
    | TranslationSource.Manual -> ApiTypes.TranslationSource.Manual

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

let private toExampleResponse (encoder: IResourceIdEncoder) (example: Example) : ExampleResponse =
    { Id = encoder.Encode(ExampleId.value example.Id)
      ExampleText = example.ExampleText
      Source = toExampleSourceDto example.Source }

let private toDefinitionResponse (encoder: IResourceIdEncoder) (definition: Definition) : DefinitionResponse =
    { Id = encoder.Encode(DefinitionId.value definition.Id)
      DefinitionText = definition.DefinitionText
      Source = toDefinitionSourceDto definition.Source
      DisplayOrder = definition.DisplayOrder
      Examples =
        definition.Examples
        |> List.map(toExampleResponse encoder) }

let private toTranslationResponse (encoder: IResourceIdEncoder) (translation: Translation) : TranslationResponse =
    { Id = encoder.Encode(TranslationId.value translation.Id)
      TranslationText = translation.TranslationText
      Source = toTranslationSourceDto translation.Source
      DisplayOrder = translation.DisplayOrder
      Examples =
        translation.Examples
        |> List.map(toExampleResponse encoder) }

let toEntryResponse (encoder: IResourceIdEncoder) (entry: Entry) : EntryResponse =
    { Id = encoder.Encode(EntryId.value entry.Id)
      VocabularyId = encoder.Encode(VocabularyId.value entry.VocabularyId)
      EntryText = entry.EntryText
      CreatedAt = entry.CreatedAt
      UpdatedAt = entry.UpdatedAt
      Definitions =
        entry.Definitions
        |> List.map(toDefinitionResponse encoder)
      Translations =
        entry.Translations
        |> List.map(toTranslationResponse encoder) }
