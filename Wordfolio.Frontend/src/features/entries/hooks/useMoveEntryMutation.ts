import { useMutation } from "@tanstack/react-query";

import { mapEntry } from "../../../shared/api/entryMappers";
import { entriesApi } from "../api/entriesApi";
import type { Entry } from "../../../shared/types/entries";

interface MoveEntryParams {
    readonly collectionId: number;
    readonly entryId: number;
    readonly sourceVocabularyId: number;
    readonly targetVocabularyId: number;
}

interface UseMoveEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: () => void;
}

export function useMoveEntryMutation(options?: UseMoveEntryMutationOptions) {
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
        onSuccess: async (data) => {
            await options?.onSuccess?.(mapEntry(data));
        },
        onError: options?.onError,
    });
}
