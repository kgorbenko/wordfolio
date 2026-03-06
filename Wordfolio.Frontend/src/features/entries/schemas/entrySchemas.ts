import { z } from "zod";

export const entryCreateRouteParamsSchema = z.object({
    collectionId: z.coerce.number().int().positive(),
    vocabularyId: z.coerce.number().int().positive(),
});

export const entryRouteParamsSchema = z.object({
    collectionId: z.coerce.number().int().positive(),
    vocabularyId: z.coerce.number().int().positive(),
    entryId: z.coerce.number().int().positive(),
});
