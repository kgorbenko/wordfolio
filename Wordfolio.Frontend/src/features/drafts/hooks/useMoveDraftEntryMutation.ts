import { useMutation, useQueryClient } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";
import type { EntryResponse, ApiError } from "../../entries/api/entriesApi";

interface MoveEntryParams {
    readonly entryId: number;
    readonly sourceVocabularyId: number;
    readonly targetVocabularyId: number;
}

interface UseMoveDraftEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
}

export function useMoveDraftEntryMutation(
    options?: UseMoveDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId, targetVocabularyId }: MoveEntryParams) =>
            draftsApi.moveDraftEntry(entryId, {
                vocabularyId: targetVocabularyId,
            }),
        onSuccess: (data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: ["drafts"],
            });
            void queryClient.invalidateQueries({
                queryKey: ["entries", variables.targetVocabularyId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["drafts", "detail", data.id],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
