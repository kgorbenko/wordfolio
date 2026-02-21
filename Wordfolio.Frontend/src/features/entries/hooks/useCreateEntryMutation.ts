import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    entriesApi,
    CreateEntryRequest,
    EntryResponse,
    ApiError,
    isDuplicateEntryError,
} from "../api/entriesApi";

interface CreateEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly request: CreateEntryRequest;
}

interface UseCreateEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
    readonly onDuplicateEntry?: (existingEntry: EntryResponse) => void;
}

export function useCreateEntryMutation(
    options?: UseCreateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            request,
        }: CreateEntryParams) =>
            entriesApi.createEntry(collectionId, vocabularyId, request),
        onSuccess: (data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: [
                    "entries",
                    variables.collectionId,
                    variables.vocabularyId,
                ],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            void queryClient.invalidateQueries({
                queryKey: ["drafts"],
            });
            options?.onSuccess?.(data);
        },
        onError: (error: ApiError) => {
            if (isDuplicateEntryError(error) && options?.onDuplicateEntry) {
                options.onDuplicateEntry(error.existingEntry!);
            } else {
                options?.onError?.(error);
            }
        },
    });
}
