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
