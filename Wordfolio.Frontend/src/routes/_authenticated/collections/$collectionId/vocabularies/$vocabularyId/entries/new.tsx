import { createFileRoute } from "@tanstack/react-router";

import { CreateEntryPage } from "../../../../../../../features/entries/pages/CreateEntryPage";
import { entryCreateRouteParamsSchema } from "../../../../../../../features/entries/schemas/entrySchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/new"
)({
    component: CreateEntryPage,
    params: {
        parse: (params) => entryCreateRouteParamsSchema.parse(params),
    },
});
