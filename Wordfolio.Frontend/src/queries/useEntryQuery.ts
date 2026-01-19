import { useQuery } from "@tanstack/react-query";

import { entriesApi } from "../api/entriesApi";

export function useEntryQuery(entryId: number) {
    return useQuery({
        queryKey: ["entries", "detail", entryId],
        queryFn: () => entriesApi.getEntry(entryId),
    });
}
