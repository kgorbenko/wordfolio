import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
    collectionsApi,
    CreateCollectionRequest,
    CollectionResponse,
    ApiError,
} from "../api/collectionsApi";

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
            collectionsApi.create(request),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({ queryKey: ["collections"] });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
