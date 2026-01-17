import { useMutation, useQueryClient } from "@tanstack/react-query";

import { vocabulariesApi, ApiError } from "../api/vocabulariesApi";

interface DeleteVocabularyParams {
    collectionId: number;
    vocabularyId: number;
}

interface UseDeleteVocabularyMutationOptions {
    onSuccess?: () => void;
    onError?: (error: ApiError) => void;
}

export function useDeleteVocabularyMutation(
    options?: UseDeleteVocabularyMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ collectionId, vocabularyId }: DeleteVocabularyParams) =>
            vocabulariesApi.deleteVocabulary(collectionId, vocabularyId),
        onSuccess: (_data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: ["vocabularies", variables.collectionId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
