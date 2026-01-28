import { createFileRoute } from "@tanstack/react-router";

import { EditEntryPage } from "../../../../../../../../features/entries/pages/EditEntryPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/edit"
)({
    component: EditEntryPage,
});
