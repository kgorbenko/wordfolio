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
            collectionId: String(collectionId),
            vocabularyId: String(vocabularyId),
        },
    };
}

export function vocabularyEditPath(collectionId: number, vocabularyId: number) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/edit" as const,
        params: {
            collectionId: String(collectionId),
            vocabularyId: String(vocabularyId),
        },
    };
}

export function vocabularyCreatePath(collectionId: number) {
    return {
        to: "/collections/$collectionId/vocabularies/new" as const,
        params: { collectionId: String(collectionId) },
    };
}
