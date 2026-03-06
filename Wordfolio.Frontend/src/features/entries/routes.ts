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
    collectionId: number | string,
    vocabularyId: number | string,
    entryId: number | string
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId" as const,
        params: {
            collectionId: Number(collectionId),
            vocabularyId: Number(vocabularyId),
            entryId: Number(entryId),
        },
    };
}

export function entryEditPath(
    collectionId: number | string,
    vocabularyId: number | string,
    entryId: number | string
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/entries/$entryId/edit" as const,
        params: {
            collectionId: Number(collectionId),
            vocabularyId: Number(vocabularyId),
            entryId: Number(entryId),
        },
    };
}

export function entryCreatePath(
    collectionId: number | string,
    vocabularyId: number | string
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/entries/new" as const,
        params: {
            collectionId: Number(collectionId),
            vocabularyId: Number(vocabularyId),
        },
    };
}

export function collectionsPath() {
    return { to: "/collections" as const };
}

export function collectionDetailPath(collectionId: number) {
    return {
        to: "/collections/$collectionId" as const,
        params: { collectionId },
    };
}

export function vocabularyDetailPath(
    collectionId: number,
    vocabularyId: number
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId" as const,
        params: {
            collectionId,
            vocabularyId,
        },
    };
}

export function draftsEntryDetailPath(entryId: number) {
    return {
        to: "/drafts/entries/$entryId" as const,
        params: { entryId },
    };
}
