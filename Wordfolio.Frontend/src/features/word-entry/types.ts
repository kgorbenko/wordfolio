export interface Definition {
    readonly text: string;
    readonly partOfSpeech: string | null;
    readonly examples: string[];
}

export interface TranslationExample {
    readonly russian: string;
    readonly english: string;
}

export interface Translation {
    readonly text: string;
    readonly partOfSpeech: string | null;
    readonly examples: TranslationExample[];
}

export interface WordLookupResult {
    readonly definitions: Definition[];
    readonly translations: Translation[];
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
