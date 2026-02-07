import { useMutation, useQueryClient } from "@tanstack/react-query";

import { entriesApi, EntryResponse, ApiError } from "../api/entriesApi";

interface MoveEntryParams {
    readonly entryId: number;
    readonly sourceVocabularyId: number;
    readonly targetVocabularyId: number;
}

interface UseMoveEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
}

export function useMoveEntryMutation(options?: UseMoveEntryMutationOptions) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId, targetVocabularyId }: MoveEntryParams) =>
            entriesApi.moveEntry(entryId, { vocabularyId: targetVocabularyId }),
        onSuccess: (data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: ["entries", variables.sourceVocabularyId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["entries", variables.targetVocabularyId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["entries", "detail", data.id],
            });
            void queryClient.invalidateQueries({ queryKey: ["drafts"] });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
