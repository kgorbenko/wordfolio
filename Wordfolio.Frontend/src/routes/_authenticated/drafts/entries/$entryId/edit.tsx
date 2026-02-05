import { createFileRoute } from "@tanstack/react-router";

import { DraftsEntryEditPage } from "../../../../../features/drafts/pages/DraftsEntryEditPage";

export const Route = createFileRoute(
    "/_authenticated/drafts/entries/$entryId/edit"
)({
    component: DraftsEntryEditPage,
});
