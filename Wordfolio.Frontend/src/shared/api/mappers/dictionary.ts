import type {
    DictionaryDefinition,
    DictionaryResult,
    DictionaryTranslation,
} from "../types/dictionary";
import type {
    LookupDefinition,
    LookupTranslation,
    WordLookupResult,
} from "../types/dictionary";

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
