import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { vocabulariesApi } from "../api/vocabulariesApi";
import { mapVocabulary } from "../api/mappers";
import { Vocabulary } from "../types";

export const useVocabularyQuery = (
    collectionId: number,
    vocabularyId: number,
    options?: Partial<UseQueryOptions<Vocabulary>>
) =>
    useQuery({
        queryKey: ["vocabulary", collectionId, vocabularyId],
        queryFn: async () => {
            const response = await vocabulariesApi.getVocabulary(
                collectionId,
                vocabularyId
            );
            return mapVocabulary(response);
        },
        ...options,
    });
