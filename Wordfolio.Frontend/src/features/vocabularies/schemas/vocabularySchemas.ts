import { z } from "zod";

export const vocabularySchema = z.object({
    name: z
        .string()
        .min(1, "Name is required")
        .max(255, "Name must be at most 255 characters"),
    description: z
        .string()
        .max(500, "Description must be at most 500 characters")
        .optional()
        .transform((val) => val?.trim() || null),
});

export type VocabularyFormInput = z.input<typeof vocabularySchema>;
export type VocabularyFormData = z.output<typeof vocabularySchema>;
