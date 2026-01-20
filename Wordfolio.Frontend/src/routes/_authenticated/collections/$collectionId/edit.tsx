import { createFileRoute } from "@tanstack/react-router";
import { EditCollectionPage } from "../../../../features/collections/pages/EditCollectionPage";

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/edit"
)({
    component: EditCollectionPage,
});
