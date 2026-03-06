import {
    DefinitionSource as ApiDefinitionSource,
    ExampleSource as ApiExampleSource,
    TranslationSource as ApiTranslationSource,
} from "./entries";
import type {
    CreateEntryRequest,
    DefinitionRequest,
    DefinitionResponse,
    EntryResponse,
    ExampleRequest,
    ExampleResponse,
    TranslationRequest,
    TranslationResponse,
    UpdateEntryRequest,
} from "./entries";
import {
    DefinitionSource,
    ExampleSource,
    TranslationSource,
} from "../types/entries";
import { assertNever } from "../utils/misc";
import type {
    CreateEntryData,
    CreateDefinitionData,
    CreateExampleData,
    CreateTranslationData,
    Definition,
    Entry,
    Example,
    Translation,
} from "../types/entries";

const mapDefinitionSource = (source: ApiDefinitionSource): DefinitionSource => {
    switch (source) {
        case ApiDefinitionSource.Api:
            return DefinitionSource.Api;
        case ApiDefinitionSource.Manual:
            return DefinitionSource.Manual;
        default:
            return assertNever(source);
    }
};

const mapTranslationSource = (
    source: ApiTranslationSource
): TranslationSource => {
    switch (source) {
        case ApiTranslationSource.Api:
            return TranslationSource.Api;
        case ApiTranslationSource.Manual:
            return TranslationSource.Manual;
        default:
            return assertNever(source);
    }
};

const mapExampleSource = (source: ApiExampleSource): ExampleSource => {
    switch (source) {
        case ApiExampleSource.Api:
            return ExampleSource.Api;
        case ApiExampleSource.Custom:
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
            return ApiDefinitionSource.Api;
        case DefinitionSource.Manual:
            return ApiDefinitionSource.Manual;
        default:
            return assertNever(source);
    }
};

const toApiTranslationSource = (
    source: TranslationSource
): ApiTranslationSource => {
    switch (source) {
        case TranslationSource.Api:
            return ApiTranslationSource.Api;
        case TranslationSource.Manual:
            return ApiTranslationSource.Manual;
        default:
            return assertNever(source);
    }
};

const toApiExampleSource = (source: ExampleSource): ApiExampleSource => {
    switch (source) {
        case ExampleSource.Api:
            return ApiExampleSource.Api;
        case ExampleSource.Custom:
            return ApiExampleSource.Custom;
        default:
            return assertNever(source);
    }
};

export const mapExample = (response: ExampleResponse): Example => ({
    id: response.id,
    exampleText: response.exampleText,
    source: mapExampleSource(response.source),
});

export const mapDefinition = (response: DefinitionResponse): Definition => ({
    id: response.id,
    definitionText: response.definitionText,
    source: mapDefinitionSource(response.source),
    displayOrder: response.displayOrder,
    examples: response.examples.map(mapExample),
});

export const mapTranslation = (response: TranslationResponse): Translation => ({
    id: response.id,
    translationText: response.translationText,
    source: mapTranslationSource(response.source),
    displayOrder: response.displayOrder,
    examples: response.examples.map(mapExample),
});

export const mapEntry = (response: EntryResponse): Entry => ({
    id: response.id,
    vocabularyId: response.vocabularyId,
    entryText: response.entryText,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
    definitions: response.definitions.map(mapDefinition),
    translations: response.translations.map(mapTranslation),
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

export const mapUpdateEntryData = (
    data: CreateEntryData
): UpdateEntryRequest => ({
    entryText: data.entryText,
    definitions: data.definitions.map(mapCreateDefinitionData),
    translations: data.translations.map(mapCreateTranslationData),
});

export const mapCreateEntryData = (
    data: CreateEntryData,
    allowDuplicate?: boolean
): CreateEntryRequest => ({
    entryText: data.entryText,
    definitions: data.definitions.map(mapCreateDefinitionData),
    translations: data.translations.map(mapCreateTranslationData),
    allowDuplicate,
});
