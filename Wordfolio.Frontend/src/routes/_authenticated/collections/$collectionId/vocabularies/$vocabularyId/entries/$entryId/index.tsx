import { createFileRoute } from "@tanstack/react-router";

import { EntryDetailPage } from "../../../../../../../../features/entries/pages/EntryDetailPage";
import { entryRouteParamsSchema } from "../../../../../../../../features/entries/schemas/entrySchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/"
)({
    component: EntryDetailPage,
    params: {
        parse: (params) => entryRouteParamsSchema.parse(params),
    },
});
