import { getRouteApi } from "@tanstack/react-router";

export const collectionRouteIds = {
    list: "/_authenticated/collections/",
    detail: "/_authenticated/collections/$collectionId/",
    edit: "/_authenticated/collections/$collectionId/edit",
    create: "/_authenticated/collections/new",
} as const;

export const collectionListRouteApi = getRouteApi(collectionRouteIds.list);
export const collectionDetailRouteApi = getRouteApi(collectionRouteIds.detail);
export const collectionEditRouteApi = getRouteApi(collectionRouteIds.edit);
export const collectionCreateRouteApi = getRouteApi(collectionRouteIds.create);

export function collectionsPath() {
    return { to: "/collections" as const };
}

export function collectionDetailPath(collectionId: number) {
    return {
        to: "/collections/$collectionId" as const,
        params: { collectionId: String(collectionId) },
    };
}

export function collectionEditPath(collectionId: number) {
    return {
        to: "/collections/$collectionId/edit" as const,
        params: { collectionId: String(collectionId) },
    };
}

export function collectionCreatePath() {
    return { to: "/collections/new" as const };
}
