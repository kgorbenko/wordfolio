import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    vocabulariesApi,
    CreateVocabularyRequest,
    VocabularyResponse,
    ApiError,
} from "../api/vocabulariesApi";

interface CreateVocabularyParams {
    collectionId: number;
    request: CreateVocabularyRequest;
}

interface UseCreateVocabularyMutationOptions {
    onSuccess?: (data: VocabularyResponse) => void;
    onError?: (error: ApiError) => void;
}

export function useCreateVocabularyMutation(
    options?: UseCreateVocabularyMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ collectionId, request }: CreateVocabularyParams) =>
            vocabulariesApi.createVocabulary(collectionId, request),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({
                queryKey: ["vocabularies", data.collectionId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
