import { useMutation, useQueryClient } from "@tanstack/react-query";

import { client } from "../client";
import { mapVocabulary } from "../mappers/vocabularies";
import type { Vocabulary } from "../types/vocabularies";

interface VocabularyInput {
    readonly name: string;
    readonly description: string | null;
}

interface CreateVocabularyMutationVariables {
    collectionId: string;
    data: VocabularyInput;
}

interface UseCreateVocabularyMutationOptions {
    onSuccess?: (data: Vocabulary) => Promise<void>;
    onError?: () => void;
}

export const useCreateVocabularyMutation = (
    options?: UseCreateVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            collectionId,
            data,
        }: CreateVocabularyMutationVariables) => {
            const { data: response, error } = await client.POST(
                "/collections/{collectionId}/vocabularies",
                {
                    params: { path: { collectionId } },
                    body: {
                        name: data.name,
                        description: data.description,
                    },
                }
            );
            if (error) throw error;
            return mapVocabulary(response!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
};

interface UpdateVocabularyMutationVariables {
    collectionId: string;
    vocabularyId: string;
    data: VocabularyInput;
}

interface UseUpdateVocabularyMutationOptions {
    onSuccess?: (data: Vocabulary) => Promise<void>;
    onError?: () => void;
}

export const useUpdateVocabularyMutation = (
    options?: UseUpdateVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            collectionId,
            vocabularyId,
            data,
        }: UpdateVocabularyMutationVariables) => {
            const { data: response, error } = await client.PUT(
                "/collections/{collectionId}/vocabularies/{id}",
                {
                    params: {
                        path: { collectionId, id: vocabularyId },
                    },
                    body: {
                        name: data.name,
                        description: data.description,
                    },
                }
            );
            if (error) throw error;
            return mapVocabulary(response!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
};

interface DeleteVocabularyMutationVariables {
    collectionId: string;
    vocabularyId: string;
}

interface UseDeleteVocabularyMutationOptions {
    onSuccess?: () => Promise<void>;
    onError?: () => void;
}

export const useDeleteVocabularyMutation = (
    options?: UseDeleteVocabularyMutationOptions
) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            collectionId,
            vocabularyId,
        }: DeleteVocabularyMutationVariables) => {
            const { error } = await client.DELETE(
                "/collections/{collectionId}/vocabularies/{id}",
                {
                    params: {
                        path: { collectionId, id: vocabularyId },
                    },
                }
            );
            if (error) throw error;
        },
        onSuccess: async () => {
            await options?.onSuccess?.();
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
};

interface MoveVocabularyParams {
    readonly sourceCollectionId: string;
    readonly vocabularyId: string;
    readonly targetCollectionId: string;
}

interface UseMoveVocabularyMutationOptions {
    readonly onSuccess?: (data: Vocabulary) => Promise<void>;
    readonly onError?: () => void;
}

export function useMoveVocabularyMutation(
    options?: UseMoveVocabularyMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            sourceCollectionId,
            vocabularyId,
            targetCollectionId,
        }: MoveVocabularyParams) => {
            const { data, error } = await client.POST(
                "/collections/{collectionId}/vocabularies/{id}/move",
                {
                    params: {
                        path: {
                            collectionId: sourceCollectionId,
                            id: vocabularyId,
                        },
                    },
                    body: { collectionId: targetCollectionId },
                }
            );
            if (error) throw error;
            return mapVocabulary(data!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
