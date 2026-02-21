import { useQuery } from "@tanstack/react-query";

import { entriesApi } from "../api/entriesApi";
import { mapEntry } from "../api/mappers";

export function useEntryQuery(
    collectionId: number,
    vocabularyId: number,
    entryId: number
) {
    return useQuery({
        queryKey: ["entries", "detail", collectionId, vocabularyId, entryId],
        queryFn: async () => {
            const response = await entriesApi.getEntry(
                collectionId,
                vocabularyId,
                entryId
            );
            return mapEntry(response);
        },
    });
}
