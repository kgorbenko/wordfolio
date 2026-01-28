import { createFileRoute } from "@tanstack/react-router";

import { CreateEntryPage } from "../../../../../../../features/entries";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/new"
)({
    component: CreateEntryPage,
});
