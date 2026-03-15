import { useMutation, useQueryClient } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import {
    mapCollectionDetail,
    mapToUpdateCollectionRequest,
} from "../api/mappers";
import { CollectionFormData } from "../schemas/collectionSchemas";
import { Collection } from "../types";

interface UseUpdateCollectionMutationOptions {
    onSuccess?: (data: Collection) => Promise<void>;
    onError?: () => void;
}

export function useUpdateCollectionMutation(
    id: number,
    options?: UseUpdateCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CollectionFormData) =>
            collectionsApi.update(id, mapToUpdateCollectionRequest(data)),
        onSuccess: async (data) => {
            await options?.onSuccess?.(mapCollectionDetail(data));
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
