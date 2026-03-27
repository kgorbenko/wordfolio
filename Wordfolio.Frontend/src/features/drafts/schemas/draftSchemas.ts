import { z } from "zod";

export const draftEntryRouteParamsSchema = z.object({
    entryId: z.coerce.number().int().positive(),
});

export const draftsListSearchParamsSchema = z.object({
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
