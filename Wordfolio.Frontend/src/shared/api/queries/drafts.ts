import { useQuery, type UseQueryOptions } from "@tanstack/react-query";

import { client } from "../client";
import { mapDraftsVocabulary } from "../mappers/drafts";
import { mapEntry } from "../mappers/entries";
import type { DraftsData } from "../types/drafts";
import type { Entry } from "../types/entries";

export const useDraftsQuery = (
    options?: Partial<UseQueryOptions<DraftsData | null>>
) =>
    useQuery<DraftsData | null>({
        queryKey: ["drafts"],
        queryFn: async () => {
            const result = await client.GET("/drafts/all");
            if (result.response.status === 404) return null;
            if (result.error) throw result.error;
            const data = result.data!;
            return {
                vocabulary: mapDraftsVocabulary(data.vocabulary),
                entries: (data.entries ?? []).map(mapEntry),
            };
        },
        ...options,
    });

export const useDraftEntryQuery = (
    entryId: number,
    options?: Partial<UseQueryOptions<Entry>>
) =>
    useQuery({
        queryKey: ["drafts", "detail", entryId],
        queryFn: async () => {
            const { data, error } = await client.GET("/drafts/{id}", {
                params: { path: { id: entryId } },
            });
            if (error) throw error;
            return mapEntry(data!);
        },
        ...options,
    });
