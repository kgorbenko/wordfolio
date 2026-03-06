import { useMutation, useQueryClient } from "@tanstack/react-query";

import { entriesApi } from "../api/entriesApi";

interface DeleteEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly entryId: number;
}

interface UseDeleteEntryMutationOptions {
    readonly onSuccess?: () => void;
    readonly onError?: () => void;
}

export function useDeleteEntryMutation(
    options?: UseDeleteEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            entryId,
        }: DeleteEntryParams) =>
            entriesApi.deleteEntry(collectionId, vocabularyId, entryId),
        onSuccess: () => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
