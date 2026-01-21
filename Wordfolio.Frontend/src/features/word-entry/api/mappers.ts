import { DictionaryResult } from "../../../api/dictionaryApi";
import { WordLookupResult, Definition, Translation } from "../types";

const mapDefinition = (
    response: DictionaryResult["definitions"][number]
): Definition => ({
    text: response.definition,
    partOfSpeech: response.partOfSpeech,
    examples: response.exampleSentences,
});

const mapTranslation = (
    response: DictionaryResult["translations"][number]
): Translation => ({
    text: response.translation,
    partOfSpeech: response.partOfSpeech,
    examples: response.examples,
});

export const mapDictionaryResult = (
    response: DictionaryResult
): WordLookupResult => ({
    definitions: response.definitions.map(mapDefinition),
    translations: response.translations.map(mapTranslation),
});
