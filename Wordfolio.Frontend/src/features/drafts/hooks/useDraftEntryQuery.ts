import { useQuery } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";
import { mapEntry } from "../../../shared/api/entryMappers";

export function useDraftEntryQuery(entryId: number) {
    return useQuery({
        queryKey: ["drafts", "detail", entryId],
        queryFn: async () => {
            const response = await draftsApi.getDraftEntry(entryId);
            return mapEntry(response);
        },
    });
}
