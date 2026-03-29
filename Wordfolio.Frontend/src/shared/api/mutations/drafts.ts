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

interface CreateDraftParams {
    readonly input: CreateEntryData;
    readonly allowDuplicate?: boolean;
}

interface UseCreateDraftMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: (error: ApiError) => void;
    readonly onDuplicateEntry?: (existingEntry: Entry) => void;
}

export function useCreateDraftMutation(
    options?: UseCreateDraftMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ input, allowDuplicate }: CreateDraftParams) => {
            const result = await client.POST("/drafts", {
                body: mapCreateEntryData(input, allowDuplicate),
            });
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

interface UpdateDraftEntryParams {
    readonly entryId: number;
    readonly data: CreateEntryData;
}

interface UseUpdateDraftEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: () => void;
}

export function useUpdateDraftEntryMutation(
    options?: UseUpdateDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ entryId, data }: UpdateDraftEntryParams) => {
            const { data: response, error } = await client.PUT("/drafts/{id}", {
                params: { path: { id: entryId } },
                body: mapUpdateEntryData(data),
            });
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

interface DeleteDraftEntryParams {
    readonly entryId: number;
}

interface UseDeleteDraftEntryMutationOptions {
    readonly onSuccess?: () => Promise<void>;
    readonly onError?: () => void;
}

export function useDeleteDraftEntryMutation(
    options?: UseDeleteDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ entryId }: DeleteDraftEntryParams) => {
            const { error } = await client.DELETE("/drafts/{id}", {
                params: { path: { id: entryId } },
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

interface MoveDraftEntryParams {
    readonly entryId: number;
    readonly targetVocabularyId: number;
}

interface UseMoveDraftEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => Promise<void>;
    readonly onError?: () => void;
}

export function useMoveDraftEntryMutation(
    options?: UseMoveDraftEntryMutationOptions
) {
    return useMutation({
        mutationFn: async ({
            entryId,
            targetVocabularyId,
        }: MoveDraftEntryParams) => {
            const { data, error } = await client.POST("/drafts/{id}/move", {
                params: { path: { id: entryId } },
                body: { vocabularyId: targetVocabularyId },
            });
            if (error) throw error;
            return mapEntry(data!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
