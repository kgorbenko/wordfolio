import type { ApiError } from "./common";

export enum DefinitionSource {
    Api = "Api",
    Manual = "Manual",
}

export enum TranslationSource {
    Api = "Api",
    Manual = "Manual",
}

export enum ExampleSource {
    Api = "Api",
    Custom = "Custom",
}

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

export interface ExampleRequest {
    readonly exampleText: string;
    readonly source: ExampleSource;
}

export interface DefinitionRequest {
    readonly definitionText: string;
    readonly source: DefinitionSource;
    readonly examples: ExampleRequest[];
}

export interface TranslationRequest {
    readonly translationText: string;
    readonly source: TranslationSource;
    readonly examples: ExampleRequest[];
}

export interface UpdateEntryRequest {
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
}

export interface MoveEntryRequest {
    readonly vocabularyId: number;
}

export interface CreateEntryRequest {
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
    readonly allowDuplicate?: boolean;
}

export interface DuplicateEntryError extends ApiError {
    readonly existingEntry: EntryResponse;
}

export const isDuplicateEntryError = (
    error: ApiError
): error is DuplicateEntryError =>
    error.status === 409 && "existingEntry" in error;
