import { createFileRoute } from "@tanstack/react-router";

import { CreateDraftPage } from "../../../features/drafts/pages/CreateDraftPage";

export const Route = createFileRoute("/_authenticated/drafts/new")({
    component: CreateDraftPage,
});
