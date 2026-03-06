import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabulariesApi } from "../api/vocabulariesApi";
import { mapVocabulary, mapToCreateVocabularyRequest } from "../api/mappers";
import { VocabularyFormData } from "../schemas/vocabularySchemas";
import { Vocabulary } from "../types";

interface CreateVocabularyMutationVariables {
    collectionId: number;
    data: VocabularyFormData;
}

interface UseCreateVocabularyMutationOptions {
    onSuccess?: (data: Vocabulary) => void;
    onError?: () => void;
}

export const useCreateVocabularyMutation = (
    options?: UseCreateVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            data,
        }: CreateVocabularyMutationVariables) =>
            vocabulariesApi.createVocabulary(
                collectionId,
                mapToCreateVocabularyRequest(data)
            ),
        onSuccess: (data) => {
            void queryClient.invalidateQueries();

            if (options?.onSuccess) {
                options.onSuccess(mapVocabulary(data));
            }
        },
        onError: options?.onError,
    });
};
