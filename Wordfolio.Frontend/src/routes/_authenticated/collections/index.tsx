import { createFileRoute } from "@tanstack/react-router";
import { CollectionsPage } from "../../../features/collections/pages/CollectionsPage";
import { collectionsListSearchParamsSchema } from "../../../features/collections/schemas/collectionSchemas";

export const Route = createFileRoute("/_authenticated/collections/")({
    component: CollectionsPage,
    validateSearch: collectionsListSearchParamsSchema,
});
