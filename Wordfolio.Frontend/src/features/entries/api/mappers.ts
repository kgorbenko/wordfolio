import {
    DictionaryDefinition,
    DictionaryTranslation,
    DictionaryResult,
} from "../../../api/dictionaryApi";
import {
    EntryResponse,
    DefinitionResponse,
    TranslationResponse,
    ExampleResponse,
} from "./entriesApi";
import {
    Entry,
    Definition,
    Translation,
    Example,
    WordLookupResult,
    LookupDefinition,
    LookupTranslation,
} from "../types";

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

const mapLookupDefinition = (
    response: DictionaryDefinition
): LookupDefinition => ({
    text: response.definition,
    partOfSpeech: response.partOfSpeech,
    examples: response.exampleSentences,
});

const mapLookupTranslation = (
    response: DictionaryTranslation
): LookupTranslation => ({
    text: response.translation,
    partOfSpeech: response.partOfSpeech,
    examples: response.examples,
});

export const mapDictionaryResult = (
    response: DictionaryResult
): WordLookupResult => ({
    definitions: response.definitions.map(mapLookupDefinition),
    translations: response.translations.map(mapLookupTranslation),
});
