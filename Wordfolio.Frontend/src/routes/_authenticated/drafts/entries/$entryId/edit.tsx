import { createFileRoute } from "@tanstack/react-router";

import { DraftsEntryEditPage } from "../../../../../features/drafts/pages/DraftsEntryEditPage";
import { draftEntryRouteParamsSchema } from "../../../../../features/drafts/schemas/draftSchemas";

export const Route = createFileRoute(
    "/_authenticated/drafts/entries/$entryId/edit"
)({
    params: {
        parse: (params) => draftEntryRouteParamsSchema.parse(params),
    },
    component: DraftsEntryEditPage,
});
