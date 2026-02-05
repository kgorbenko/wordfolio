export type DefinitionSource = "Api" | "Manual";
export type TranslationSource = "Api" | "Manual";
export type ExampleSource = "Api" | "Custom";

export interface ExampleResponse {
    readonly id: number;
    readonly exampleText: string;
    readonly source: ExampleSource;
}

export interface DefinitionResponse {
    readonly id: number;
    readonly definitionText: string;
    readonly source: DefinitionSource;
    readonly displayOrder: number;
    readonly examples: ExampleResponse[];
}

export interface TranslationResponse {
    readonly id: number;
    readonly translationText: string;
    readonly source: TranslationSource;
    readonly displayOrder: number;
    readonly examples: ExampleResponse[];
}

export interface EntryResponse {
    readonly id: number;
    readonly vocabularyId: number;
    readonly entryText: string;
    readonly createdAt: string;
    readonly updatedAt: string | null;
    readonly definitions: DefinitionResponse[];
    readonly translations: TranslationResponse[];
}
