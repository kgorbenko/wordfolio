import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabulariesApi } from "../api/vocabulariesApi";

interface DeleteVocabularyMutationVariables {
    collectionId: number;
    vocabularyId: number;
}

interface UseDeleteVocabularyMutationOptions {
    onSuccess?: () => Promise<void>;
    onError?: () => void;
}

export const useDeleteVocabularyMutation = (
    options?: UseDeleteVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
        }: DeleteVocabularyMutationVariables) =>
            vocabulariesApi.deleteVocabulary(collectionId, vocabularyId),
        onSuccess: async () => {
            await options?.onSuccess?.();
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
};
