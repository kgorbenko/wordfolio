import { z } from "zod";

const trimmed = (val: string) => val === val.trim();

export const exampleSchema = z.object({
    exampleText: z
        .string()
        .min(1, "Example text is required")
        .max(500, "Example must be at most 500 characters")
        .refine(trimmed, {
            message: "Cannot have leading or trailing whitespace",
        }),
    source: z.enum(["Api", "Custom"]),
});

export const definitionSchema = z.object({
    definitionText: z
        .string()
        .min(1, "Definition text is required")
        .max(255, "Definition must be at most 255 characters")
        .refine(trimmed, {
            message: "Cannot have leading or trailing whitespace",
        }),
    source: z.enum(["Api", "Manual"]),
    examples: z.array(exampleSchema),
});

export const translationSchema = z.object({
    translationText: z
        .string()
        .min(1, "Translation text is required")
        .max(255, "Translation must be at most 255 characters")
        .refine(trimmed, {
            message: "Cannot have leading or trailing whitespace",
        }),
    source: z.enum(["Api", "Manual"]),
    examples: z.array(exampleSchema),
});

export const entrySchema = z
    .object({
        entryText: z
            .string()
            .min(1, "Entry text is required")
            .max(255, "Entry text must be at most 255 characters")
            .refine(trimmed, {
                message: "Cannot have leading or trailing whitespace",
            }),
        definitions: z.array(definitionSchema),
        translations: z.array(translationSchema),
    })
    .refine(
        (data) => data.definitions.length > 0 || data.translations.length > 0,
        {
            message: "At least one definition or translation is required",
            path: ["definitions"],
        }
    );

export const exampleFormSchema = exampleSchema.extend({
    id: z.string(),
});

export const definitionFormSchema = definitionSchema.extend({
    id: z.string(),
    examples: z.array(exampleFormSchema),
});

export const translationFormSchema = translationSchema.extend({
    id: z.string(),
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

export type ExampleSource = "Api" | "Custom";
export type DefinitionSource = "Api" | "Manual";

export type ExampleFormInput = z.input<typeof exampleSchema>;
export type ExampleFormData = z.output<typeof exampleSchema>;

export type DefinitionFormInput = z.input<typeof definitionSchema>;
export type DefinitionFormData = z.output<typeof definitionSchema>;

export type TranslationFormInput = z.input<typeof translationSchema>;
export type TranslationFormData = z.output<typeof translationSchema>;

export type EntryFormInput = z.input<typeof entryFormSchema>;
export type EntryFormData = z.output<typeof entryFormSchema>;
