import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { vocabulariesApi } from "../api/vocabulariesApi";
import { mapVocabularyDetail } from "../api/mappers";
import { VocabularyDetail } from "../types";

export const useVocabularyQuery = (
    collectionId: number,
    vocabularyId: number,
    options?: Partial<UseQueryOptions<VocabularyDetail>>
) =>
    useQuery({
        queryKey: ["vocabulary", collectionId, vocabularyId],
        queryFn: async () => {
            const response = await vocabulariesApi.getVocabulary(
                collectionId,
                vocabularyId
            );
            return mapVocabularyDetail(response);
        },
        ...options,
    });
