import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    vocabulariesApi,
    UpdateVocabularyRequest,
    VocabularyResponse,
    ApiError,
} from "../api/vocabulariesApi";

interface UpdateVocabularyParams {
    collectionId: number;
    vocabularyId: number;
    request: UpdateVocabularyRequest;
}

interface UseUpdateVocabularyMutationOptions {
    onSuccess?: (data: VocabularyResponse) => void;
    onError?: (error: ApiError) => void;
}

export function useUpdateVocabularyMutation(
    options?: UseUpdateVocabularyMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            request,
        }: UpdateVocabularyParams) =>
            vocabulariesApi.updateVocabulary(
                collectionId,
                vocabularyId,
                request
            ),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({
                queryKey: ["vocabularies", data.collectionId],
            });
            void queryClient.invalidateQueries({
                queryKey: ["vocabularies", data.collectionId, data.id],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
