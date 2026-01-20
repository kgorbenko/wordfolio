import { createFileRoute } from "@tanstack/react-router";
import { CollectionDetailPage } from "../../../../features/collections/pages/CollectionDetailPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/"
)({
    component: CollectionDetailPage,
});
