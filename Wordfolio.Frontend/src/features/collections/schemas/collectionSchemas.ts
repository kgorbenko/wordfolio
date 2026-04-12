import { z } from "zod";

export const collectionSchema = z.object({
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

export const collectionIdRouteParamsSchema = z.object({
    collectionId: z.string().min(1),
});

export type CollectionFormInput = z.input<typeof collectionSchema>;
export type CollectionFormData = z.output<typeof collectionSchema>;

export const collectionsListSearchParamsSchema = z.object({
    sortField: z
        .enum(["name", "createdAt", "updatedAt", "vocabularyCount"])
        .optional(),
    sortDirection: z.enum(["asc", "desc"]).optional(),
    filter: z.string().optional(),
});

export type CollectionSortField = NonNullable<
    z.infer<typeof collectionsListSearchParamsSchema>["sortField"]
>;

export const vocabulariesListSearchParamsSchema = z.object({
    sortField: z
        .enum(["name", "createdAt", "updatedAt", "entryCount"])
        .optional(),
    sortDirection: z.enum(["asc", "desc"]).optional(),
    filter: z.string().optional(),
});

export type VocabularySortField = NonNullable<
    z.infer<typeof vocabulariesListSearchParamsSchema>["sortField"]
>;
