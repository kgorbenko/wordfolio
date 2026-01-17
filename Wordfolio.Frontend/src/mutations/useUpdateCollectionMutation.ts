import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    vocabulariesApi,
    UpdateCollectionRequest,
    CollectionResponse,
    ApiError,
} from "../api/vocabulariesApi";

interface UpdateCollectionParams {
    id: number;
    request: UpdateCollectionRequest;
}

interface UseUpdateCollectionMutationOptions {
    onSuccess?: (data: CollectionResponse) => void;
    onError?: (error: ApiError) => void;
}

export function useUpdateCollectionMutation(
    options?: UseUpdateCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, request }: UpdateCollectionParams) =>
            vocabulariesApi.updateCollection(id, request),
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
