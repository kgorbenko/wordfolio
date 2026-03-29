import type { components } from "../generated/schema";
import {
    DefinitionSource,
    ExampleSource,
    TranslationSource,
} from "../types/entries";
import type {
    CreateDefinitionData,
    CreateEntryData,
    CreateExampleData,
    CreateTranslationData,
    Definition,
    Entry,
    Example,
    Translation,
} from "../types/entries";
import { assertNever } from "../../utils/misc";

type ApiDefinitionSource = components["schemas"]["DefinitionSource"];
type ApiTranslationSource = components["schemas"]["TranslationSource"];
type ApiExampleSource = components["schemas"]["ExampleSource"];
type ExampleResponse = components["schemas"]["ExampleResponse"];
type DefinitionResponse = components["schemas"]["DefinitionResponse"];
type TranslationResponse = components["schemas"]["TranslationResponse"];
type EntryResponse = components["schemas"]["EntryResponse"];
type ExampleRequest = components["schemas"]["ExampleRequest"];
type DefinitionRequest = components["schemas"]["DefinitionRequest"];
type TranslationRequest = components["schemas"]["TranslationRequest"];

const mapDefinitionSourceFromApi = (
    source: ApiDefinitionSource
): DefinitionSource => {
    switch (source) {
        case "Api":
            return DefinitionSource.Api;
        case "Manual":
            return DefinitionSource.Manual;
        default:
            return assertNever(source);
    }
};

const mapTranslationSourceFromApi = (
    source: ApiTranslationSource
): TranslationSource => {
    switch (source) {
        case "Api":
            return TranslationSource.Api;
        case "Manual":
            return TranslationSource.Manual;
        default:
            return assertNever(source);
    }
};

const mapExampleSourceFromApi = (source: ApiExampleSource): ExampleSource => {
    switch (source) {
        case "Api":
            return ExampleSource.Api;
        case "Custom":
            return ExampleSource.Custom;
        default:
            return assertNever(source);
    }
};

const toApiDefinitionSource = (
    source: DefinitionSource
): ApiDefinitionSource => {
    switch (source) {
        case DefinitionSource.Api:
            return "Api";
        case DefinitionSource.Manual:
            return "Manual";
        default:
            return assertNever(source);
    }
};

const toApiTranslationSource = (
    source: TranslationSource
): ApiTranslationSource => {
    switch (source) {
        case TranslationSource.Api:
            return "Api";
        case TranslationSource.Manual:
            return "Manual";
        default:
            return assertNever(source);
    }
};

const toApiExampleSource = (source: ExampleSource): ApiExampleSource => {
    switch (source) {
        case ExampleSource.Api:
            return "Api";
        case ExampleSource.Custom:
            return "Custom";
        default:
            return assertNever(source);
    }
};

export const mapExample = (response: ExampleResponse): Example => ({
    id: response.id,
    exampleText: response.exampleText,
    source: mapExampleSourceFromApi(response.source),
});

export const mapDefinition = (response: DefinitionResponse): Definition => ({
    id: response.id,
    definitionText: response.definitionText,
    source: mapDefinitionSourceFromApi(response.source),
    displayOrder: response.displayOrder,
    examples: (response.examples ?? []).map(mapExample),
});

export const mapTranslation = (response: TranslationResponse): Translation => ({
    id: response.id,
    translationText: response.translationText,
    source: mapTranslationSourceFromApi(response.source),
    displayOrder: response.displayOrder,
    examples: (response.examples ?? []).map(mapExample),
});

export const mapEntry = (response: EntryResponse): Entry => ({
    id: response.id,
    vocabularyId: response.vocabularyId,
    entryText: response.entryText,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
    definitions: (response.definitions ?? []).map(mapDefinition),
    translations: (response.translations ?? []).map(mapTranslation),
});

const mapCreateExampleData = (data: CreateExampleData): ExampleRequest => ({
    exampleText: data.exampleText,
    source: toApiExampleSource(data.source),
});

const mapCreateDefinitionData = (
    data: CreateDefinitionData
): DefinitionRequest => ({
    definitionText: data.definitionText,
    source: toApiDefinitionSource(data.source),
    examples: data.examples.map(mapCreateExampleData),
});

const mapCreateTranslationData = (
    data: CreateTranslationData
): TranslationRequest => ({
    translationText: data.translationText,
    source: toApiTranslationSource(data.source),
    examples: data.examples.map(mapCreateExampleData),
});

export const mapCreateEntryData = (
    data: CreateEntryData,
    allowDuplicate?: boolean
): components["schemas"]["CreateEntryRequest"] => ({
    entryText: data.entryText,
    definitions: data.definitions.map(mapCreateDefinitionData),
    translations: data.translations.map(mapCreateTranslationData),
    allowDuplicate: allowDuplicate as boolean,
});

export const mapUpdateEntryData = (
    data: CreateEntryData
): components["schemas"]["UpdateEntryRequest"] => ({
    entryText: data.entryText,
    definitions: data.definitions.map(mapCreateDefinitionData),
    translations: data.translations.map(mapCreateTranslationData),
});
