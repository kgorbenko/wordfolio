import { useQuery } from "@tanstack/react-query";

import { entriesApi } from "../api/entriesApi";
import { mapEntry } from "../api/mappers";

export function useEntryQuery(entryId: number) {
    return useQuery({
        queryKey: ["entries", "detail", entryId],
        queryFn: async () => {
            const response = await entriesApi.getEntry(entryId);
            return mapEntry(response);
        },
    });
}
