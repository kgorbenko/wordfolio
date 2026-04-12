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

export const vocabularyCreateRouteParamsSchema = z.object({
    collectionId: z.string().min(1),
});

export const vocabularyRouteParamsSchema = z.object({
    collectionId: z.string().min(1),
    vocabularyId: z.string().min(1),
});

export type VocabularyFormInput = z.input<typeof vocabularySchema>;
export type VocabularyFormData = z.output<typeof vocabularySchema>;

export const entriesListSearchParamsSchema = z.object({
    sortField: z
        .enum([
            "entryText",
            "createdAt",
            "updatedAt",
            "translationCount",
            "definitionCount",
        ])
        .optional(),
    sortDirection: z.enum(["asc", "desc"]).optional(),
    filter: z.string().optional(),
});

export type EntrySortField = NonNullable<
    z.infer<typeof entriesListSearchParamsSchema>["sortField"]
>;
