import { useQuery } from "@tanstack/react-query";

import { entriesApi } from "../api/entriesApi";

export function useEntriesQuery(vocabularyId: number) {
    return useQuery({
        queryKey: ["entries", vocabularyId],
        queryFn: () => entriesApi.getEntries(vocabularyId),
    });
}
