import { createFileRoute } from "@tanstack/react-router";

import { DraftsEntryDetailPage } from "../../../../../features/drafts/pages/DraftsEntryDetailPage";
import { draftEntryRouteParamsSchema } from "../../../../../features/drafts/schemas/draftSchemas";

export const Route = createFileRoute(
    "/_authenticated/drafts/entries/$entryId/"
)({
    params: {
        parse: (params) => draftEntryRouteParamsSchema.parse(params),
    },
    component: DraftsEntryDetailPage,
});
