export type DefinitionSource = "Api" | "Manual";
export type TranslationSource = "Api" | "Manual";
export type ExampleSource = "Api" | "Custom";

export interface Example {
    readonly id: number;
    readonly exampleText: string;
    readonly source: ExampleSource;
}

export interface Definition {
    readonly id: number;
    readonly definitionText: string;
    readonly source: DefinitionSource;
    readonly displayOrder: number;
    readonly examples: Example[];
}

export interface Translation {
    readonly id: number;
    readonly translationText: string;
    readonly source: TranslationSource;
    readonly displayOrder: number;
    readonly examples: Example[];
}

export interface Entry {
    readonly id: number;
    readonly vocabularyId: number;
    readonly entryText: string;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
    readonly definitions: Definition[];
    readonly translations: Translation[];
}
