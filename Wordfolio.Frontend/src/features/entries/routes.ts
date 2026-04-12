import { getRouteApi } from "@tanstack/react-router";

export const entryRouteIds = {
    detail: "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/",
    edit: "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/edit",
    create: "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/new",
} as const;

export const entryDetailRouteApi = getRouteApi(entryRouteIds.detail);
export const entryEditRouteApi = getRouteApi(entryRouteIds.edit);
export const entryCreateRouteApi = getRouteApi(entryRouteIds.create);

export function entryDetailPath(
    collectionId: string,
    vocabularyId: string,
    entryId: string
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId" as const,
        params: {
            collectionId,
            vocabularyId,
            entryId,
        },
    };
}

export function entryEditPath(
    collectionId: string,
    vocabularyId: string,
    entryId: string
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/edit" as const,
        params: {
            collectionId,
            vocabularyId,
            entryId,
        },
    };
}

export function entryCreatePath(collectionId: string, vocabularyId: string) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/entries/new" as const,
        params: {
            collectionId,
            vocabularyId,
        },
    };
}
