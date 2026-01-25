import { createFileRoute } from "@tanstack/react-router";
import { CreateEntryPage } from "../../../../../features/word-entry/pages/CreateEntryPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/$vocabularyId/new"
)({
    component: CreateEntryPage,
});
