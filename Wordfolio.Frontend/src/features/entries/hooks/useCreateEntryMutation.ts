import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    entriesApi,
    CreateEntryRequest,
    EntryResponse,
    ApiError,
} from "../api/entriesApi";

interface UseCreateEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
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
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
