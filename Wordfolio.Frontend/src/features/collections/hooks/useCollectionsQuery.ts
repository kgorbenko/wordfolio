import { useQuery } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import { mapCollectionWithVocabularyCount } from "../api/mappers";

export const useCollectionsQuery = () =>
    useQuery({
        queryKey: ["collections"],
        queryFn: async () => {
            const response = await collectionsApi.getHierarchyCollections();
            return response.map(mapCollectionWithVocabularyCount);
        },
    });
