import { useQuery, UseQueryOptions } from "@tanstack/react-query";

import { mapVocabularyCollectionContext } from "../api/mappers";
import { vocabulariesApi } from "../api/vocabulariesApi";
import { VocabularyCollectionContext } from "../types";

export const useVocabularyCollectionQuery = (
    collectionId: number,
    options?: Partial<UseQueryOptions<VocabularyCollectionContext>>
) =>
    useQuery({
        queryKey: ["collection", collectionId],
        queryFn: async () => {
            const response = await vocabulariesApi.getCollection(collectionId);
            return mapVocabularyCollectionContext(response);
        },
        ...options,
    });
