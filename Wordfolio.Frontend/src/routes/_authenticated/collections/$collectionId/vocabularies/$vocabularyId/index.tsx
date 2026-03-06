import { createFileRoute } from "@tanstack/react-router";

import { VocabularyDetailPage } from "../../../../../../features/vocabularies/pages/VocabularyDetailPage";
import { vocabularyRouteParamsSchema } from "../../../../../../features/vocabularies/schemas/vocabularySchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/"
)({
    component: VocabularyDetailPage,
    params: {
        parse: (params) => vocabularyRouteParamsSchema.parse(params),
    },
});
