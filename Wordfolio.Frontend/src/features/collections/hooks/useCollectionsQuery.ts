import { useQuery } from "@tanstack/react-query";
import {
    collectionsApi,
    SearchUserCollectionsQuery,
} from "../api/collectionsApi";
import { mapCollectionOverview } from "../api/mappers";

export const useCollectionsQuery = (query: SearchUserCollectionsQuery) =>
    useQuery({
        queryKey: ["collections", query],
        queryFn: async () => {
            const response =
                await collectionsApi.getHierarchyCollections(query);
            return response.map(mapCollectionOverview);
        },
    });
