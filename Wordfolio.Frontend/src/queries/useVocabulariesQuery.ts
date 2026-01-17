import { useQuery } from "@tanstack/react-query";

import { vocabulariesApi } from "../api/vocabulariesApi";

export function useVocabulariesQuery(collectionId: number) {
    return useQuery({
        queryKey: ["vocabularies", collectionId],
        queryFn: () => vocabulariesApi.getVocabularies(collectionId),
    });
}
