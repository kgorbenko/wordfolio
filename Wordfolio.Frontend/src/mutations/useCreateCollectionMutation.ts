import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    vocabulariesApi,
    CreateCollectionRequest,
    CollectionResponse,
    ApiError,
} from "../api/vocabulariesApi";

interface UseCreateCollectionMutationOptions {
    onSuccess?: (data: CollectionResponse) => void;
    onError?: (error: ApiError) => void;
}

export function useCreateCollectionMutation(
    options?: UseCreateCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (request: CreateCollectionRequest) =>
            vocabulariesApi.createCollection(request),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({ queryKey: ["collections"] });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
