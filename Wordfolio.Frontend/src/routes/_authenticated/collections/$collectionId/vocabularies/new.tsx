import { createFileRoute } from "@tanstack/react-router";

import { CreateVocabularyPage } from "../../../../../features/vocabularies/pages/CreateVocabularyPage";
import { vocabularyCreateRouteParamsSchema } from "../../../../../features/vocabularies/schemas/vocabularySchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/new"
)({
    component: CreateVocabularyPage,
    params: {
        parse: (params) => vocabularyCreateRouteParamsSchema.parse(params),
    },
});
