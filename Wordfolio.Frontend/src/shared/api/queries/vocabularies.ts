import { useQuery, type UseQueryOptions } from "@tanstack/react-query";

import { client } from "../client";
import {
    mapVocabularyCollectionContext,
    mapVocabularyDetail,
} from "../mappers/vocabularies";
import type {
    VocabularyCollectionContext,
    VocabularyDetail,
} from "../types/vocabularies";

export const useVocabularyDetailQuery = (
    collectionId: string,
    vocabularyId: string,
    options?: Partial<UseQueryOptions<VocabularyDetail>>
) =>
    useQuery({
        queryKey: ["vocabulary", collectionId, vocabularyId],
        queryFn: async () => {
            const { data, error } = await client.GET(
                "/collections/{collectionId}/vocabularies/{id}",
                { params: { path: { collectionId, id: vocabularyId } } }
            );
            if (error) throw error;
            return mapVocabularyDetail(data!);
        },
        ...options,
    });

export const useVocabularyCollectionQuery = (
    collectionId: string,
    options?: Partial<UseQueryOptions<VocabularyCollectionContext>>
) =>
    useQuery({
        queryKey: ["collection", collectionId],
        queryFn: async () => {
            const { data, error } = await client.GET("/collections/{id}", {
                params: { path: { id: collectionId } },
            });
            if (error) throw error;
            return mapVocabularyCollectionContext(data!);
        },
        ...options,
    });
