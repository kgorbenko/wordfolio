import { useQuery, UseQueryOptions } from "@tanstack/react-query";

import { entriesApi } from "../api/entriesApi";
import { mapEntry } from "../api/mappers";
import { Entry } from "../types";

export const useEntriesQuery = (
    vocabularyId: number,
    options?: Partial<UseQueryOptions<Entry[]>>
) =>
    useQuery({
        queryKey: ["entries", vocabularyId],
        queryFn: async () => {
            const response = await entriesApi.getEntries(vocabularyId);
            return response.map(mapEntry);
        },
        ...options,
    });
