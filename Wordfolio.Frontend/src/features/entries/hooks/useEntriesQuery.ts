import { useQuery, UseQueryOptions } from "@tanstack/react-query";

import { entriesApi } from "../api/entriesApi";
import { mapEntry } from "../api/mappers";
import { Entry } from "../types";

export const useEntriesQuery = (
    collectionId: number,
    vocabularyId: number,
    options?: Partial<UseQueryOptions<Entry[]>>
) =>
    useQuery({
        queryKey: ["entries", collectionId, vocabularyId],
        queryFn: async () => {
            const response = await entriesApi.getEntries(
                collectionId,
                vocabularyId
            );
            return response.map(mapEntry);
        },
        ...options,
    });
