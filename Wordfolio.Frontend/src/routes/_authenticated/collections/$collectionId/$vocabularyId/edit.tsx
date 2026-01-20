import { createFileRoute } from "@tanstack/react-router";
import { EditVocabularyPage } from "../../../../../features/vocabularies/pages/EditVocabularyPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/$vocabularyId/edit"
)({
    component: EditVocabularyPage,
});
