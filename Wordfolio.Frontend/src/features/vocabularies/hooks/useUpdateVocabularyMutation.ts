import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabulariesApi, UpdateVocabularyRequest, ApiError } from "../api/vocabulariesApi";
import { mapVocabulary } from "../api/mappers";
import { Vocabulary } from "../types";

interface UpdateVocabularyMutationVariables {
    collectionId: number;
    vocabularyId: number;
    request: UpdateVocabularyRequest;
}

interface UseUpdateVocabularyMutationOptions {
    onSuccess?: (data: Vocabulary) => void;
    onError?: (error: ApiError) => void;
}

export const useUpdateVocabularyMutation = (
    options?: UseUpdateVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            request,
        }: UpdateVocabularyMutationVariables) =>
            vocabulariesApi.updateVocabulary(collectionId, vocabularyId, request),
        onSuccess: (data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: [
                    "vocabulary",
                    variables.collectionId,
                    variables.vocabularyId,
                ],
            });
            void queryClient.invalidateQueries({
                queryKey: ["vocabularies", variables.collectionId],
            });
            void queryClient.invalidateQueries({ queryKey: ["collections"] });

            if (options?.onSuccess) {
                options.onSuccess(mapVocabulary(data));
            }
        },
        onError: options?.onError,
    });
};
