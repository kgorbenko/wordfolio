import {
    EntryResponse,
    DefinitionResponse,
    TranslationResponse,
    ExampleResponse,
} from "./entriesApi";
import { Entry, Definition, Translation, Example } from "../types";

export const mapExample = (response: ExampleResponse): Example => ({
    id: response.id,
    exampleText: response.exampleText,
    source: response.source,
});

export const mapDefinition = (response: DefinitionResponse): Definition => ({
    id: response.id,
    definitionText: response.definitionText,
    source: response.source,
    displayOrder: response.displayOrder,
    examples: response.examples.map(mapExample),
});

export const mapTranslation = (response: TranslationResponse): Translation => ({
    id: response.id,
    translationText: response.translationText,
    source: response.source,
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
