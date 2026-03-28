import type { components } from "../generated/schema";

export enum DefinitionSource {
    Api = 0,
    Manual = 1,
}

export enum TranslationSource {
    Api = 0,
    Manual = 1,
}

export enum ExampleSource {
    Api = 0,
    Custom = 1,
}

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

export type AnnotatedItemColor = "primary" | "secondary";

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
    readonly source: TranslationSource;
    readonly examples: ExampleItem[];
}

export interface CreateExampleData {
    readonly exampleText: string;
    readonly source: ExampleSource;
}

export interface CreateDefinitionData {
    readonly definitionText: string;
    readonly source: DefinitionSource;
    readonly examples: CreateExampleData[];
}

export interface CreateTranslationData {
    readonly translationText: string;
    readonly source: TranslationSource;
    readonly examples: CreateExampleData[];
}

export interface CreateEntryData {
    readonly entryText: string;
    readonly definitions: CreateDefinitionData[];
    readonly translations: CreateTranslationData[];
}

export interface EntryFormValues {
    readonly entryText: string;
    readonly definitions: DefinitionItem[];
    readonly translations: TranslationItem[];
}

export interface ApiError {
    readonly type?: string;
    readonly title?: string;
    readonly status?: number;
    readonly errors?: Record<string, string[]>;
}

export interface DuplicateEntryError extends ApiError {
    readonly existingEntry: components["schemas"]["EntryResponse"];
}

export const isDuplicateEntryError = (
    error: ApiError
): error is DuplicateEntryError =>
    error.status === 409 && "existingEntry" in error;
