import { createFileRoute } from "@tanstack/react-router";

import { EntryDetailPage } from "../../../../../../../../features/entries/pages/EntryDetailPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/"
)({
    component: EntryDetailPage,
});
