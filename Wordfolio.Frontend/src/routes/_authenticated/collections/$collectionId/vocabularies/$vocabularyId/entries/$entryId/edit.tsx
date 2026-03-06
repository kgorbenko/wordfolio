import { createFileRoute } from "@tanstack/react-router";

import { EditEntryPage } from "../../../../../../../../features/entries/pages/EditEntryPage";
import { entryRouteParamsSchema } from "../../../../../../../../features/entries/schemas/entrySchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/edit"
)({
    component: EditEntryPage,
    params: {
        parse: (params) => entryRouteParamsSchema.parse(params),
    },
});
