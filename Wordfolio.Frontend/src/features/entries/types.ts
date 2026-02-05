import { DefinitionRequest, TranslationRequest } from "./api/entriesApi";

export {
    type DefinitionSource,
    type TranslationSource,
    type ExampleSource,
    type Example,
    type Definition,
    type Translation,
    type Entry,
} from "../../types/entry";

import type { DefinitionSource, ExampleSource } from "../../types/entry";

export interface ExampleItem {
    readonly id: string;
    readonly exampleText: string;
    readonly source: ExampleSource;
}

export interface DefinitionItem {
    readonly id: string;
    readonly definitionText: string;
    readonly source: DefinitionSource;
    readonly examples: ExampleItem[];
}

export interface TranslationItem {
    readonly id: string;
    readonly translationText: string;
    readonly source: DefinitionSource;
    readonly examples: ExampleItem[];
}

export interface EntryFormValues {
    readonly entryText: string;
    readonly definitions: DefinitionItem[];
    readonly translations: TranslationItem[];
}

export interface EntryFormOutput {
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
}

export type AnnotatedItemColor = "primary" | "secondary";

export interface LookupDefinition {
    readonly text: string;
    readonly partOfSpeech: string | null;
    readonly examples: string[];
}

export interface LookupTranslationExample {
    readonly russian: string;
    readonly english: string;
}

export interface LookupTranslation {
    readonly text: string;
    readonly partOfSpeech: string | null;
    readonly examples: LookupTranslationExample[];
}

export interface WordLookupResult {
    readonly definitions: LookupDefinition[];
    readonly translations: LookupTranslation[];
}

export type LookupState =
    | { readonly status: "idle" }
    | { readonly status: "loading"; readonly streamingText: string }
    | { readonly status: "success"; readonly result: WordLookupResult }
    | { readonly status: "error" }
    | { readonly status: "empty" };

export interface UseWordLookupResult {
    readonly word: string;
    readonly lookupState: LookupState;
    readonly setWord: (value: string) => void;
    readonly clear: () => void;
    readonly reset: () => void;
}
