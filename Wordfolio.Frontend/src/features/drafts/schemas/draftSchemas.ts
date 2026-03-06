import { z } from "zod";

export const draftEntryRouteParamsSchema = z.object({
    entryId: z.coerce.number().int().positive(),
});
