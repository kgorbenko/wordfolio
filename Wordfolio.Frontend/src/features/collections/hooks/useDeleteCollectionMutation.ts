import { useMutation, useQueryClient } from "@tanstack/react-query";
import { collectionsApi, ApiError } from "../api/collectionsApi";

interface UseDeleteCollectionMutationOptions {
    onSuccess?: () => void;
    onError?: (error: ApiError) => void;
}

export function useDeleteCollectionMutation(
    options?: UseDeleteCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: number) => collectionsApi.delete(id),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: ["collections"] });
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
