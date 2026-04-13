import { z } from "zod";

export const entryCreateRouteParamsSchema = z.object({
    collectionId: z.string().min(1),
    vocabularyId: z.string().min(1),
});

export const entryRouteParamsSchema = z.object({
    collectionId: z.string().min(1),
    vocabularyId: z.string().min(1),
    entryId: z.string().min(1),
});
