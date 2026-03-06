import { createFileRoute } from "@tanstack/react-router";
import { EditCollectionPage } from "../../../../features/collections/pages/EditCollectionPage";
import { collectionIdRouteParamsSchema } from "../../../../features/collections/schemas/collectionSchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/edit"
)({
    component: EditCollectionPage,
    params: {
        parse: (params) => collectionIdRouteParamsSchema.parse(params),
    },
});
