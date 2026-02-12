import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    entriesApi,
    CreateEntryRequest,
    EntryResponse,
    ApiError,
    isDuplicateEntryError,
} from "../api/entriesApi";

interface UseCreateEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
    readonly onDuplicateEntry?: (existingEntry: EntryResponse) => void;
}

export function useCreateEntryMutation(
    options?: UseCreateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (request: CreateEntryRequest) =>
            entriesApi.createEntry(request),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({
                queryKey: ["entries", data.vocabularyId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            void queryClient.invalidateQueries({
                queryKey: ["drafts"],
            });
            options?.onSuccess?.(data);
        },
        onError: (error: ApiError) => {
            if (isDuplicateEntryError(error) && options?.onDuplicateEntry) {
                options.onDuplicateEntry(error.existingEntry!);
            } else {
                options?.onError?.(error);
            }
        },
    });
}
