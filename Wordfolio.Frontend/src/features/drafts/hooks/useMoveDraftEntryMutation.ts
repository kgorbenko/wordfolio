import { useMutation, useQueryClient } from "@tanstack/react-query";

import { mapEntry } from "../../../shared/api/entryMappers";
import { draftsApi } from "../api/draftsApi";
import type { Entry } from "../../../shared/types/entries";

interface MoveEntryParams {
    readonly entryId: number;
    readonly targetVocabularyId: number;
}

interface UseMoveDraftEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => void;
    readonly onError?: () => void;
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
        onSuccess: (data) => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.(mapEntry(data));
        },
        onError: options?.onError,
    });
}
