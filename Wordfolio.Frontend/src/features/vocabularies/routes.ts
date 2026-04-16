import { getRouteApi } from "@tanstack/react-router";

export const vocabularyRouteIds = {
    detail: "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/",
    edit: "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/edit",
    create: "/_authenticated/collections/$collectionId/vocabularies/new",
    practice:
        "/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/practice",
} as const;

export const vocabularyDetailRouteApi = getRouteApi(vocabularyRouteIds.detail);
export const vocabularyEditRouteApi = getRouteApi(vocabularyRouteIds.edit);
export const vocabularyCreateRouteApi = getRouteApi(vocabularyRouteIds.create);
export const practiceRouteApi = getRouteApi(vocabularyRouteIds.practice);

export function vocabularyDetailPath(
    collectionId: string,
    vocabularyId: string
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId" as const,
        params: {
            collectionId,
            vocabularyId,
        },
    };
}

export function vocabularyEditPath(collectionId: string, vocabularyId: string) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/edit" as const,
        params: {
            collectionId,
            vocabularyId,
        },
    };
}

export function vocabularyCreatePath(collectionId: string) {
    return {
        to: "/collections/$collectionId/vocabularies/new" as const,
        params: { collectionId },
    };
}

export function vocabularyPracticePath(
    collectionId: string,
    vocabularyId: string
) {
    return {
        to: "/collections/$collectionId/vocabularies/$vocabularyId/practice" as const,
        params: {
            collectionId,
            vocabularyId,
        },
    };
}
