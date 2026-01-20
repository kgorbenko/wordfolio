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

export type CollectionFormInput = z.input<typeof collectionSchema>;
export type CollectionFormData = z.output<typeof collectionSchema>;
