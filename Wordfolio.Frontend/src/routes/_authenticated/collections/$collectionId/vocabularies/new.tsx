import { createFileRoute } from "@tanstack/react-router";

import { CreateVocabularyPage } from "../../../../../features/vocabularies/pages/CreateVocabularyPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/new"
)({
    component: CreateVocabularyPage,
});
