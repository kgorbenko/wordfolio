import { createFileRoute } from "@tanstack/react-router";
import { CreateCollectionPage } from "../../../features/collections/pages/CreateCollectionPage";

export const Route = createFileRoute("/_authenticated/collections/new")({
    component: CreateCollectionPage,
});
