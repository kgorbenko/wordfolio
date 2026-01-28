import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    entriesApi,
    UpdateEntryRequest,
    EntryResponse,
    ApiError,
} from "../api/entriesApi";

interface UpdateEntryParams {
    readonly entryId: number;
    readonly request: UpdateEntryRequest;
}

interface UseUpdateEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
}

export function useUpdateEntryMutation(
    options?: UseUpdateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId, request }: UpdateEntryParams) =>
            entriesApi.updateEntry(entryId, request),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({
                queryKey: ["entries", data.vocabularyId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["entries", "detail", data.id],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
