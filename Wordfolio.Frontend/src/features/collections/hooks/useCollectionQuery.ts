import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import { mapCollectionDetail } from "../api/mappers";
import { Collection } from "../types";

export const useCollectionQuery = (
    id: number,
    options?: Partial<UseQueryOptions<Collection>>
) =>
    useQuery({
        queryKey: ["collections", id],
        queryFn: async () => {
            const response = await collectionsApi.getById(id);
            return mapCollectionDetail(response);
        },
        ...options,
    });
