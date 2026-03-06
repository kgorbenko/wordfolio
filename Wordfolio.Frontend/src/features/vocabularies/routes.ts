import { getRouteApi } from "@tanstack/react-router";

export const vocabularyRouteIds = {
    detail: "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/",
    edit: "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/edit",
    create: "/_authenticated/collections/$collectionId/vocabularies/new",
} as const;

export const vocabularyDetailRouteApi = getRouteApi(vocabularyRouteIds.detail);
export const vocabularyEditRouteApi = getRouteApi(vocabularyRouteIds.edit);
export const vocabularyCreateRouteApi = getRouteApi(vocabularyRouteIds.create);

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

export function vocabularyEditPath(collectionId: number, vocabularyId: number) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/edit" as const,
        params: {
            collectionId,
            vocabularyId,
        },
    };
}

export function vocabularyCreatePath(collectionId: number) {
    return {
        to: "/collections/$collectionId/vocabularies/new" as const,
        params: { collectionId },
    };
}

export function vocabularyCollectionsPath() {
    return { to: "/collections" as const };
}

export function vocabularyCollectionDetailPath(collectionId: number) {
    return {
        to: "/collections/$collectionId" as const,
        params: { collectionId },
    };
}

export function vocabularyEntryDetailPath(
    collectionId: number,
    vocabularyId: number,
    entryId: number
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

export function vocabularyEntryCreatePath(
    collectionId: number,
    vocabularyId: number
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/entries/new" as const,
        params: {
            collectionId,
            vocabularyId,
        },
    };
}
