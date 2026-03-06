import { useMutation, useQueryClient } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";

interface UseDeleteCollectionMutationOptions {
    onSuccess?: () => void;
    onError?: () => void;
}

export function useDeleteCollectionMutation(
    options?: UseDeleteCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: number) => collectionsApi.delete(id),
        onSuccess: () => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
