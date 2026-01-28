import { createFileRoute } from "@tanstack/react-router";

import { VocabularyDetailPage } from "../../../../../../features/vocabularies/pages/VocabularyDetailPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/"
)({
    component: VocabularyDetailPage,
});
