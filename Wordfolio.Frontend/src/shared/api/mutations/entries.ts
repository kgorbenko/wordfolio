import { useMutation, useQueryClient } from "@tanstack/react-query";

import { client } from "../client";
import {
    mapCreateEntryData,
    mapEntry,
    mapUpdateEntryData,
} from "../mappers/entries";
import type {
    ApiError,
    CreateEntryData,
    DuplicateEntryError,
    Entry,
} from "../types/entries";
import { isDuplicateEntryError } from "../types/entries";
import type { components } from "../generated/schema";

interface CreateEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly input: CreateEntryData;
    readonly allowDuplicate?: boolean;
}

interface UseCreateEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: (error: ApiError) => void;
    readonly onDuplicateEntry?: (existingEntry: Entry) => void;
}

export function useCreateEntryMutation(
    options?: UseCreateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            collectionId,
            vocabularyId,
            input,
            allowDuplicate,
        }: CreateEntryParams) => {
            const result = await client.POST(
                "/collections/{collectionId}/vocabularies/{vocabularyId}/entries",
                {
                    params: { path: { collectionId, vocabularyId } },
                    body: mapCreateEntryData(input, allowDuplicate),
                }
            );
            if (result.response.status === 409) {
                throw {
                    ...(result.error as unknown as object),
                    status: 409,
                } as DuplicateEntryError;
            }
            if (result.error) throw result.error;
            return mapEntry(result.data!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: (error: ApiError) => {
            if (isDuplicateEntryError(error) && options?.onDuplicateEntry) {
                options.onDuplicateEntry(
                    mapEntry(
                        (error as DuplicateEntryError)
                            .existingEntry as components["schemas"]["EntryResponse"]
                    )
                );
            } else {
                options?.onError?.(error);
            }
        },
    });
}

interface UpdateEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly entryId: number;
    readonly data: CreateEntryData;
}

interface UseUpdateEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: () => void;
}

export function useUpdateEntryMutation(
    options?: UseUpdateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            collectionId,
            vocabularyId,
            entryId,
            data,
        }: UpdateEntryParams) => {
            const { data: response, error } = await client.PUT(
                "/collections/{collectionId}/vocabularies/{vocabularyId}/entries/{id}",
                {
                    params: {
                        path: { collectionId, vocabularyId, id: entryId },
                    },
                    body: mapUpdateEntryData(data),
                }
            );
            if (error) throw error;
            return mapEntry(response!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}

interface DeleteEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly entryId: number;
}

interface UseDeleteEntryMutationOptions {
    readonly onSuccess?: () => Promise<void>;
    readonly onError?: () => void;
}

export function useDeleteEntryMutation(
    options?: UseDeleteEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            collectionId,
            vocabularyId,
            entryId,
        }: DeleteEntryParams) => {
            const { error } = await client.DELETE(
                "/collections/{collectionId}/vocabularies/{vocabularyId}/entries/{id}",
                {
                    params: {
                        path: { collectionId, vocabularyId, id: entryId },
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
}

interface MoveEntryParams {
    readonly collectionId: number;
    readonly entryId: number;
    readonly sourceVocabularyId: number;
    readonly targetVocabularyId: number | undefined;
}

interface UseMoveEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: () => void;
}

export function useMoveEntryMutation(options?: UseMoveEntryMutationOptions) {
    return useMutation({
        mutationFn: async ({
            collectionId,
            entryId,
            sourceVocabularyId,
            targetVocabularyId,
        }: MoveEntryParams) => {
            const { data, error } = await client.POST(
                "/collections/{collectionId}/vocabularies/{vocabularyId}/entries/{id}/move",
                {
                    params: {
                        path: {
                            collectionId,
                            vocabularyId: sourceVocabularyId,
                            id: entryId,
                        },
                    },
                    body: { vocabularyId: targetVocabularyId },
                }
            );
            if (error) throw error;
            return mapEntry(data!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
