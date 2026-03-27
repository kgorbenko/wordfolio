import { createFileRoute } from "@tanstack/react-router";
import { CollectionDetailPage } from "../../../../features/collections/pages/CollectionDetailPage";
import {
    collectionIdRouteParamsSchema,
    vocabulariesListSearchParamsSchema,
} from "../../../../features/collections/schemas/collectionSchemas";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/"
)({
    component: CollectionDetailPage,
    params: {
        parse: (params) => collectionIdRouteParamsSchema.parse(params),
    },
    validateSearch: vocabulariesListSearchParamsSchema,
});
