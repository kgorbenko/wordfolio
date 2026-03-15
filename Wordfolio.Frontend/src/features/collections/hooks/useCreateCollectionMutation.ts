import { useMutation, useQueryClient } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import {
    mapCollectionDetail,
    mapToCreateCollectionRequest,
} from "../api/mappers";
import { CollectionFormData } from "../schemas/collectionSchemas";
import { Collection } from "../types";

interface UseCreateCollectionMutationOptions {
    onSuccess?: (data: Collection) => Promise<void> | void;
    onError?: () => void;
}

export function useCreateCollectionMutation(
    options?: UseCreateCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CollectionFormData) =>
            collectionsApi.create(mapToCreateCollectionRequest(data)),
        onSuccess: async (data) => {
            await options?.onSuccess?.(mapCollectionDetail(data));
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
