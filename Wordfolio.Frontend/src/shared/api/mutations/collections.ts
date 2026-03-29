import { useMutation, useQueryClient } from "@tanstack/react-query";

import { client } from "../client";
import { mapCollectionDetail } from "../mappers/collections";
import type { Collection } from "../types/collections";

interface CollectionInput {
    readonly name: string;
    readonly description: string | null;
}

interface UseCreateCollectionMutationOptions {
    onSuccess?: (data: Collection) => Promise<void>;
    onError?: () => void;
}

export function useCreateCollectionMutation(
    options?: UseCreateCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (data: CollectionInput) => {
            const { data: response, error } = await client.POST(
                "/collections",
                {
                    body: {
                        name: data.name,
                        description: data.description,
                    },
                }
            );
            if (error) throw error;
            return mapCollectionDetail(response!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}

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
        mutationFn: async (data: CollectionInput) => {
            const { data: response, error } = await client.PUT(
                "/collections/{id}",
                {
                    params: { path: { id } },
                    body: {
                        name: data.name,
                        description: data.description,
                    },
                }
            );
            if (error) throw error;
            return mapCollectionDetail(response!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}

interface UseDeleteCollectionMutationOptions {
    onSuccess?: () => Promise<void>;
    onError?: () => void;
}

export function useDeleteCollectionMutation(
    options?: UseDeleteCollectionMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (id: number) => {
            const { error } = await client.DELETE("/collections/{id}", {
                params: { path: { id } },
            });
            if (error) throw error;
        },
        onSuccess: async () => {
            await options?.onSuccess?.();
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
