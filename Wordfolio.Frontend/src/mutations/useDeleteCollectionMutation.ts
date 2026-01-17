import { useMutation, useQueryClient } from "@tanstack/react-query";

import { vocabulariesApi, ApiError } from "../api/vocabulariesApi";

interface UseDeleteCollectionMutationOptions {
    onSuccess?: () => void;
    onError?: (error: ApiError) => void;
}

export function useDeleteCollectionMutation(
    options?: UseDeleteCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: number) => vocabulariesApi.deleteCollection(id),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: ["collections"] });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
