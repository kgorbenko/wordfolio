import { useMutation, useQueryClient } from "@tanstack/react-query";
import { vocabulariesApi, CreateVocabularyRequest, ApiError } from "../api/vocabulariesApi";
import { mapVocabulary } from "../api/mappers";
import { Vocabulary } from "../types";

interface CreateVocabularyMutationVariables {
    collectionId: number;
    request: CreateVocabularyRequest;
}

interface UseCreateVocabularyMutationOptions {
    onSuccess?: (data: Vocabulary) => void;
    onError?: (error: ApiError) => void;
}

export const useCreateVocabularyMutation = (
    options?: UseCreateVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            request,
        }: CreateVocabularyMutationVariables) =>
            vocabulariesApi.createVocabulary(collectionId, request),
        onSuccess: (data, variables) => {
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
