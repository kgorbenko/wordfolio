import { useQuery, UseQueryOptions } from "@tanstack/react-query";

import { vocabularyApi } from "../api/vocabularies";
import { mapVocabularyDetail } from "../api/vocabularyMappers";
import type { VocabularyDetail } from "../types/vocabularies";

export const useVocabularyDetailQuery = (
    collectionId: number,
    vocabularyId: number,
    options?: Partial<UseQueryOptions<VocabularyDetail>>
) =>
    useQuery({
        queryKey: ["vocabulary", collectionId, vocabularyId],
        queryFn: async () => {
            const response = await vocabularyApi.getVocabularyDetail(
                collectionId,
                vocabularyId
            );
            return mapVocabularyDetail(response);
        },
        ...options,
    });
