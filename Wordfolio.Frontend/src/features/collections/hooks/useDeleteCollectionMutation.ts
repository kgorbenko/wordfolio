import { useMutation, useQueryClient } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";

interface UseDeleteCollectionMutationOptions {
    onSuccess?: () => Promise<void> | void;
    onError?: () => void;
}

export function useDeleteCollectionMutation(
    options?: UseDeleteCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: number) => collectionsApi.delete(id),
        onSuccess: async () => {
            await options?.onSuccess?.();
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
