import { useMutation, useQueryClient } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";

interface DeleteEntryParams {
    readonly entryId: number;
}

interface UseDeleteDraftEntryMutationOptions {
    readonly onSuccess?: () => Promise<void> | void;
    readonly onError?: () => void;
}

export function useDeleteDraftEntryMutation(
    options?: UseDeleteDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId }: DeleteEntryParams) =>
            draftsApi.deleteDraftEntry(entryId),
        onSuccess: async () => {
            await options?.onSuccess?.();
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
