import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { ApiError } from "../api/common";
import { isDuplicateEntryError } from "../api/entries";
import { createDraft } from "../api/draftsApi";
import { mapCreateEntryData, mapEntry } from "../api/entryMappers";
import type { CreateEntryData, Entry } from "../types/entries";

interface CreateDraftParams {
    readonly input: CreateEntryData;
    readonly allowDuplicate?: boolean;
}

interface UseCreateDraftMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void> | void;
    readonly onError?: (error: ApiError) => void;
    readonly onDuplicateEntry?: (existingEntry: Entry) => void;
}

export function useCreateDraftMutation(
    options?: UseCreateDraftMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ input, allowDuplicate }: CreateDraftParams) =>
            createDraft(mapCreateEntryData(input, allowDuplicate)),
        onSuccess: async (data) => {
            await options?.onSuccess?.(mapEntry(data));
            void queryClient.invalidateQueries();
        },
        onError: (error: ApiError) => {
            if (isDuplicateEntryError(error) && options?.onDuplicateEntry) {
                options.onDuplicateEntry(mapEntry(error.existingEntry));
            } else {
                options?.onError?.(error);
            }
        },
    });
}
