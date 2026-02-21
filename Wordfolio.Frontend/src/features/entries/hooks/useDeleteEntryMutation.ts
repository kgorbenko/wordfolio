import { useMutation, useQueryClient } from "@tanstack/react-query";

import { entriesApi, ApiError } from "../api/entriesApi";

interface DeleteEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly entryId: number;
}

interface UseDeleteEntryMutationOptions {
    readonly onSuccess?: () => void;
    readonly onError?: (error: ApiError) => void;
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
        onSuccess: (_data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: [
                    "entries",
                    variables.collectionId,
                    variables.vocabularyId,
                ],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            void queryClient.invalidateQueries({
                queryKey: ["drafts"],
            });
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
