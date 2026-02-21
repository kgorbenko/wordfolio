import { useMutation, useQueryClient } from "@tanstack/react-query";

import { entriesApi, EntryResponse, ApiError } from "../api/entriesApi";

interface MoveEntryParams {
    readonly collectionId: number;
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
        mutationFn: ({
            collectionId,
            entryId,
            sourceVocabularyId,
            targetVocabularyId,
        }: MoveEntryParams) =>
            entriesApi.moveEntry(collectionId, sourceVocabularyId, entryId, {
                vocabularyId: targetVocabularyId,
            }),
        onSuccess: (data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: [
                    "entries",
                    variables.collectionId,
                    variables.sourceVocabularyId,
                ],
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
