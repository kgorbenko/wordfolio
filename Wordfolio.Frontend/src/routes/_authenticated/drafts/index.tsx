import { createFileRoute } from "@tanstack/react-router";

import { DraftsPage } from "../../../features/drafts/pages/DraftsPage";
import { draftsListSearchParamsSchema } from "../../../features/drafts/schemas/draftSchemas";

export const Route = createFileRoute("/_authenticated/drafts/")({
    component: DraftsPage,
    validateSearch: draftsListSearchParamsSchema,
});
