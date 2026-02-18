import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
    ApiError,
    CreateEntryRequest,
    EntryResponse,
    isDuplicateEntryError,
} from "../../entries/api/entriesApi";
import { draftsApi } from "../api/draftsApi";

interface UseCreateDraftMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
    readonly onDuplicateEntry?: (existingEntry: EntryResponse) => void;
}

export function useCreateDraftMutation(
    options?: UseCreateDraftMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (request: CreateEntryRequest) =>
            draftsApi.createDraft({
                entryText: request.entryText,
                definitions: request.definitions,
                translations: request.translations,
                allowDuplicate: request.allowDuplicate,
            }),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({
                queryKey: ["drafts"],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
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
