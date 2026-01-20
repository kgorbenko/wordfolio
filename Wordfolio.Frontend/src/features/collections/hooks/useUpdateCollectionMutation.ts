import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    collectionsApi,
    UpdateCollectionRequest,
    CollectionResponse,
    ApiError,
} from "../api/collectionsApi";

interface UseUpdateCollectionMutationOptions {
    onSuccess?: (data: CollectionResponse) => void;
    onError?: (error: ApiError) => void;
}

export function useUpdateCollectionMutation(
    id: number,
    options?: UseUpdateCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (request: UpdateCollectionRequest) =>
            collectionsApi.update(id, request),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({ queryKey: ["collections"] });
            void queryClient.invalidateQueries({
                queryKey: ["collections", id],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
