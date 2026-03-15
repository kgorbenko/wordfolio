import { useMutation, useQueryClient } from "@tanstack/react-query";

import { vocabulariesApi } from "../api/vocabulariesApi";
import type { VocabularyResponse } from "../api/vocabulariesApi";

interface MoveVocabularyParams {
    readonly sourceCollectionId: number;
    readonly vocabularyId: number;
    readonly targetCollectionId: number;
}

interface UseMoveVocabularyMutationOptions {
    readonly onSuccess?: (data: VocabularyResponse) => Promise<void>;
    readonly onError?: () => void;
}

export function useMoveVocabularyMutation(
    options?: UseMoveVocabularyMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            sourceCollectionId,
            vocabularyId,
            targetCollectionId,
        }: MoveVocabularyParams) =>
            vocabulariesApi.moveVocabulary(sourceCollectionId, vocabularyId, {
                collectionId: targetCollectionId,
            }),
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
