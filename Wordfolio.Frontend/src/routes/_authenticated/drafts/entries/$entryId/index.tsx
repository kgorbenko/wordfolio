import { createFileRoute } from "@tanstack/react-router";

import { DraftsEntryDetailPage } from "../../../../../features/drafts/pages/DraftsEntryDetailPage";

export const Route = createFileRoute(
    "/_authenticated/drafts/entries/$entryId/"
)({
    component: DraftsEntryDetailPage,
});
