import { useQuery, UseQueryOptions } from "@tanstack/react-query";

import { mapVocabularyEntryPreviews } from "../api/mappers";
import { vocabulariesApi } from "../api/vocabulariesApi";
import { VocabularyEntryPreview } from "../types";

export const useVocabularyEntriesQuery = (
    collectionId: number,
    vocabularyId: number,
    options?: Partial<UseQueryOptions<VocabularyEntryPreview[]>>
) =>
    useQuery({
        queryKey: ["entries", collectionId, vocabularyId],
        queryFn: async () => {
            const response = await vocabulariesApi.getVocabularyEntries(
                collectionId,
                vocabularyId
            );
            return mapVocabularyEntryPreviews(response);
        },
        ...options,
    });
