import { createFileRoute } from "@tanstack/react-router";

import { EditVocabularyPage } from "../../../../../../features/vocabularies/pages/EditVocabularyPage";
import { vocabularyRouteParamsSchema } from "../../../../../../features/vocabularies/schemas/vocabularySchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/edit"
)({
    component: EditVocabularyPage,
    params: {
        parse: (params) => vocabularyRouteParamsSchema.parse(params),
    },
});
