import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { ApiError } from "../api/common";
import { isDuplicateEntryError } from "../api/entries";
import { createEntry } from "../api/entriesApi";
import { mapCreateEntryData, mapEntry } from "../api/entryMappers";
import type { CreateEntryData, Entry } from "../types/entries";

interface CreateEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly input: CreateEntryData;
    readonly allowDuplicate?: boolean;
}

interface UseCreateEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => void;
    readonly onError?: (error: ApiError) => void;
    readonly onDuplicateEntry?: (existingEntry: Entry) => void;
}

export function useCreateEntryMutation(
    options?: UseCreateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            input,
            allowDuplicate,
        }: CreateEntryParams) =>
            createEntry(
                collectionId,
                vocabularyId,
                mapCreateEntryData(input, allowDuplicate)
            ),
        onSuccess: (data) => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.(mapEntry(data));
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
