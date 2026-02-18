import { useQuery } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";
import { mapEntry } from "../../entries/api/mappers";

export function useDraftEntryQuery(entryId: number) {
    return useQuery({
        queryKey: ["drafts", "detail", entryId],
        queryFn: async () => {
            const response = await draftsApi.getDraftEntry(entryId);
            return mapEntry(response);
        },
    });
}
