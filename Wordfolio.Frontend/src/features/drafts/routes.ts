import { getRouteApi } from "@tanstack/react-router";

export const draftsRouteIds = {
    list: "/_authenticated/drafts/",
    create: "/_authenticated/drafts/new",
    entryDetail: "/_authenticated/drafts/entries/$entryId/",
    entryEdit: "/_authenticated/drafts/entries/$entryId/edit",
} as const;

export const draftsListRouteApi = getRouteApi(draftsRouteIds.list);
export const draftsCreateRouteApi = getRouteApi(draftsRouteIds.create);
export const draftsEntryDetailRouteApi = getRouteApi(
    draftsRouteIds.entryDetail
);
export const draftsEntryEditRouteApi = getRouteApi(draftsRouteIds.entryEdit);

export function draftsPath() {
    return { to: "/drafts" as const };
}

export function draftsCreatePath() {
    return { to: "/drafts/new" as const };
}

export function draftsEntryDetailPath(entryId: number) {
    return {
        to: "/drafts/entries/$entryId" as const,
        params: { entryId: String(entryId) },
    };
}

export function draftsEntryEditPath(entryId: number) {
    return {
        to: "/drafts/entries/$entryId/edit" as const,
        params: { entryId: String(entryId) },
    };
}
