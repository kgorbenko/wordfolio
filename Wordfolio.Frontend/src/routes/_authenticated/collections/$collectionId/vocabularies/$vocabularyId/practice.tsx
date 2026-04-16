import { createFileRoute } from "@tanstack/react-router";

import { PracticePage } from "../../../../../../features/vocabularies/pages/PracticePage";
import { vocabularyRouteParamsSchema } from "../../../../../../features/vocabularies/schemas/vocabularySchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/practice"
)({
    component: PracticePage,
    params: {
        parse: (params) => vocabularyRouteParamsSchema.parse(params),
    },
});
