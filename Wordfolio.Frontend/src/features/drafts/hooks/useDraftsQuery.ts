import { useQuery } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";
import { mapEntry } from "../../../shared/api/entryMappers";
import { mapDraftsVocabulary } from "../api/mappers";
import type { DraftsData } from "../types";

export const useDraftsQuery = () =>
    useQuery<DraftsData | null>({
        queryKey: ["drafts"],
        queryFn: async () => {
            const response = await draftsApi.getDrafts();
            if (!response) return null;
            return {
                vocabulary: mapDraftsVocabulary(response.vocabulary),
                entries: response.entries.map(mapEntry),
            };
        },
    });
