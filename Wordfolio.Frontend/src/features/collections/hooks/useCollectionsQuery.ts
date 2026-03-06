import { useQuery } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import {
    mapCollectionWithVocabularyCount,
    mapToSearchUserCollectionsQuery,
} from "../api/mappers";
import { CollectionSearchQuery } from "../types";

export const useCollectionsQuery = (query: CollectionSearchQuery) =>
    useQuery({
        queryKey: ["collections", query],
        queryFn: async () => {
            const response = await collectionsApi.getHierarchyCollections(
                mapToSearchUserCollectionsQuery(query)
            );
            return response.map(mapCollectionWithVocabularyCount);
        },
    });
