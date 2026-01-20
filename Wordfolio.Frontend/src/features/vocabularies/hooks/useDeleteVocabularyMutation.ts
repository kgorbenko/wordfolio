import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabulariesApi, ApiError } from "../api/vocabulariesApi";

interface DeleteVocabularyMutationVariables {
    collectionId: number;
    vocabularyId: number;
}

interface UseDeleteVocabularyMutationOptions {
    onSuccess?: () => void;
    onError?: (error: ApiError) => void;
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
        onSuccess: (_data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: ["vocabularies", variables.collectionId],
            });
            void queryClient.invalidateQueries({ queryKey: ["collections"] });

            if (options?.onSuccess) {
                options.onSuccess();
            }
        },
        onError: options?.onError,
    });
};
