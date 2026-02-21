import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    entriesApi,
    UpdateEntryRequest,
    EntryResponse,
    ApiError,
} from "../api/entriesApi";

interface UpdateEntryParams {
    readonly collectionId: number;
    readonly vocabularyId: number;
    readonly entryId: number;
    readonly request: UpdateEntryRequest;
}

interface UseUpdateEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
}

export function useUpdateEntryMutation(
    options?: UseUpdateEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            collectionId,
            vocabularyId,
            entryId,
            request,
        }: UpdateEntryParams) =>
            entriesApi.updateEntry(
                collectionId,
                vocabularyId,
                entryId,
                request
            ),
        onSuccess: (data, variables) => {
            void queryClient.invalidateQueries({
                queryKey: [
                    "entries",
                    variables.collectionId,
                    variables.vocabularyId,
                ],
            });
            void queryClient.invalidateQueries({
                queryKey: [
                    "entries",
                    "detail",
                    variables.collectionId,
                    variables.vocabularyId,
                    variables.entryId,
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
        onError: options?.onError,
    });
}
