import { useQuery } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import { mapCollections } from "../api/mappers";

export const useCollectionsQuery = () =>
    useQuery({
        queryKey: ["collections"],
        queryFn: async () => {
            const response = await collectionsApi.getAll();
            return mapCollections(response);
        },
    });
