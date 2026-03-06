import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabulariesApi } from "../api/vocabulariesApi";

interface DeleteVocabularyMutationVariables {
    collectionId: number;
    vocabularyId: number;
}

interface UseDeleteVocabularyMutationOptions {
    onSuccess?: () => void;
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
        onSuccess: () => {
            void queryClient.invalidateQueries();

            if (options?.onSuccess) {
                options.onSuccess();
            }
        },
        onError: options?.onError,
    });
};
