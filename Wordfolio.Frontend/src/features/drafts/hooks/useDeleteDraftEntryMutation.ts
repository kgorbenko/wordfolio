import { useMutation, useQueryClient } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";
import type { ApiError } from "../../entries/api/entriesApi";

interface DeleteEntryParams {
    readonly entryId: number;
    readonly vocabularyId: number;
}

interface UseDeleteDraftEntryMutationOptions {
    readonly onSuccess?: () => void;
    readonly onError?: (error: ApiError) => void;
}

export function useDeleteDraftEntryMutation(
    options?: UseDeleteDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId }: DeleteEntryParams) =>
            draftsApi.deleteDraftEntry(entryId),
        onSuccess: () => {
            void queryClient.invalidateQueries({
                queryKey: ["drafts"],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
