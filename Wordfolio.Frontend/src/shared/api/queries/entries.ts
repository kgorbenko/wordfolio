import { useQuery, type UseQueryOptions } from "@tanstack/react-query";

import { client } from "../client";
import { mapEntry } from "../mappers/entries";
import type { Entry } from "../types/entries";

export const useVocabularyEntriesQuery = (
    collectionId: string,
    vocabularyId: string,
    options?: Partial<UseQueryOptions<Entry[]>>
) =>
    useQuery({
        queryKey: ["entries", collectionId, vocabularyId],
        queryFn: async () => {
            const { data, error } = await client.GET(
                "/collections/{collectionId}/vocabularies/{vocabularyId}/entries",
                { params: { path: { collectionId, vocabularyId } } }
            );
            if (error) throw error;
            return data!.map(mapEntry);
        },
        ...options,
    });

export const useEntryQuery = (
    collectionId: string,
    vocabularyId: string,
    entryId: string,
    options?: Partial<UseQueryOptions<Entry>>
) =>
    useQuery({
        queryKey: ["entries", "detail", collectionId, vocabularyId, entryId],
        queryFn: async () => {
            const { data, error } = await client.GET(
                "/collections/{collectionId}/vocabularies/{vocabularyId}/entries/{id}",
                {
                    params: {
                        path: { collectionId, vocabularyId, id: entryId },
                    },
                }
            );
            if (error) throw error;
            return mapEntry(data!);
        },
        ...options,
    });
