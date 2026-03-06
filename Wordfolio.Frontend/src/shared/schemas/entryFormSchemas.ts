import { z } from "zod";

import {
    DefinitionSource,
    ExampleSource,
    TranslationSource,
} from "../types/entries";

const trimmed = (value: string) => value === value.trim();

export const exampleFormSchema = z.object({
    id: z.string(),
    exampleText: z
        .string()
        .min(1, "Example text is required")
        .max(500, "Example must be at most 500 characters")
        .refine(trimmed, {
            message: "Cannot have leading or trailing whitespace",
        }),
    source: z.nativeEnum(ExampleSource),
});

export const definitionFormSchema = z.object({
    id: z.string(),
    definitionText: z
        .string()
        .min(1, "Definition text is required")
        .max(255, "Definition must be at most 255 characters")
        .refine(trimmed, {
            message: "Cannot have leading or trailing whitespace",
        }),
    source: z.nativeEnum(DefinitionSource),
    examples: z.array(exampleFormSchema),
});

export const translationFormSchema = z.object({
    id: z.string(),
    translationText: z
        .string()
        .min(1, "Translation text is required")
        .max(255, "Translation must be at most 255 characters")
        .refine(trimmed, {
            message: "Cannot have leading or trailing whitespace",
        }),
    source: z.nativeEnum(TranslationSource),
    examples: z.array(exampleFormSchema),
});

export const entryFormSchema = z
    .object({
        entryText: z
            .string()
            .min(1, "Entry text is required")
            .max(255, "Entry text must be at most 255 characters")
            .refine(trimmed, {
                message: "Cannot have leading or trailing whitespace",
            }),
        definitions: z.array(definitionFormSchema),
        translations: z.array(translationFormSchema),
    })
    .refine(
        (data) => data.definitions.length > 0 || data.translations.length > 0,
        {
            message: "At least one definition or translation is required",
            path: ["definitions"],
        }
    );

export type EntryFormInput = z.input<typeof entryFormSchema>;
export type EntryFormData = z.output<typeof entryFormSchema>;
