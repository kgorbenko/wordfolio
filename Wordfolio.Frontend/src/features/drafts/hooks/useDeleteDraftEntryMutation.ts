import { useMutation, useQueryClient } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";

interface DeleteEntryParams {
    readonly entryId: number;
}

interface UseDeleteDraftEntryMutationOptions {
    readonly onSuccess?: () => void;
    readonly onError?: () => void;
}

export function useDeleteDraftEntryMutation(
    options?: UseDeleteDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId }: DeleteEntryParams) =>
            draftsApi.deleteDraftEntry(entryId),
        onSuccess: () => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
