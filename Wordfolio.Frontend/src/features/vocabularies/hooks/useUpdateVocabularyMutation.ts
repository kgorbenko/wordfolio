import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabulariesApi } from "../api/vocabulariesApi";
import { mapVocabulary, mapToUpdateVocabularyRequest } from "../api/mappers";
import { VocabularyFormData } from "../schemas/vocabularySchemas";
import { Vocabulary } from "../types";

interface UpdateVocabularyMutationVariables {
    collectionId: number;
    vocabularyId: number;
    data: VocabularyFormData;
}

interface UseUpdateVocabularyMutationOptions {
    onSuccess?: (data: Vocabulary) => void;
    onError?: () => void;
}

export const useUpdateVocabularyMutation = (
    options?: UseUpdateVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            data,
        }: UpdateVocabularyMutationVariables) =>
            vocabulariesApi.updateVocabulary(
                collectionId,
                vocabularyId,
                mapToUpdateVocabularyRequest(data)
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
