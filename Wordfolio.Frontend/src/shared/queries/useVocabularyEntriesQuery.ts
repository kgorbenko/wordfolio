import { useQuery, UseQueryOptions } from "@tanstack/react-query";

import { mapEntry } from "../api/entryMappers";
import { vocabularyApi } from "../api/vocabularies";
import { Entry } from "../types/entries";

export const useVocabularyEntriesQuery = (
    collectionId: number,
    vocabularyId: number,
    options?: Partial<UseQueryOptions<Entry[]>>
) =>
    useQuery({
        queryKey: ["entries", collectionId, vocabularyId],
        queryFn: async () => {
            const response = await vocabularyApi.getVocabularyEntries(
                collectionId,
                vocabularyId
            );
            return response.map(mapEntry);
        },
        ...options,
    });
