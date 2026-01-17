import { useQuery } from "@tanstack/react-query";

import { vocabulariesApi } from "../api/vocabulariesApi";

export function useVocabularyQuery(collectionId: number, vocabularyId: number) {
    return useQuery({
        queryKey: ["vocabularies", collectionId, vocabularyId],
        queryFn: () =>
            vocabulariesApi.getVocabulary(collectionId, vocabularyId),
    });
}
