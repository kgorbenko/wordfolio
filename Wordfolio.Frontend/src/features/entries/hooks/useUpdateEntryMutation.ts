import { useMutation, useQueryClient } from "@tanstack/react-query";

import { mapEntry, mapUpdateEntryData } from "../../../shared/api/entryMappers";
import { entriesApi } from "../api/entriesApi";
import type { CreateEntryData, Entry } from "../../../shared/types/entries";

interface UpdateEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly entryId: number;
    readonly data: CreateEntryData;
}

interface UseUpdateEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: () => void;
}

export function useUpdateEntryMutation(
    options?: UseUpdateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            entryId,
            data,
        }: UpdateEntryParams) =>
            entriesApi.updateEntry(
                collectionId,
                vocabularyId,
                entryId,
                mapUpdateEntryData(data)
            ),
        onSuccess: async (data) => {
            await options?.onSuccess?.(mapEntry(data));
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
