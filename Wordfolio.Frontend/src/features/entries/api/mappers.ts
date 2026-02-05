import {
    DictionaryDefinition,
    DictionaryTranslation,
    DictionaryResult,
} from "../../../api/dictionaryApi";
import {
    WordLookupResult,
    LookupDefinition,
    LookupTranslation,
} from "../types";

export {
    mapExample,
    mapDefinition,
    mapTranslation,
    mapEntry,
} from "../../../api/entryMappers";

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
