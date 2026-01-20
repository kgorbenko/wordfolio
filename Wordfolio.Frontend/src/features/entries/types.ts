import { ExampleSource, DefinitionSource } from "./schemas/entrySchemas";
import { DefinitionRequest, TranslationRequest } from "../../api/entriesApi";

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
