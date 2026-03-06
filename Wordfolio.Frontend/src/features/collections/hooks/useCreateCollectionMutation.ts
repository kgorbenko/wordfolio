import { useMutation, useQueryClient } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import {
    mapCollectionDetail,
    mapToCreateCollectionRequest,
} from "../api/mappers";
import { CollectionFormData } from "../schemas/collectionSchemas";
import { Collection } from "../types";

interface UseCreateCollectionMutationOptions {
    onSuccess?: (data: Collection) => void;
    onError?: () => void;
}

export function useCreateCollectionMutation(
    options?: UseCreateCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CollectionFormData) =>
            collectionsApi.create(mapToCreateCollectionRequest(data)),
        onSuccess: (data) => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.(mapCollectionDetail(data));
        },
        onError: options?.onError,
    });
}
