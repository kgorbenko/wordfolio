import { useMutation, useQueryClient } from "@tanstack/react-query";

import { draftsApi } from "../api/draftsApi";
import type {
    UpdateEntryRequest,
    EntryResponse,
    ApiError,
} from "../../entries/api/entriesApi";

interface UpdateEntryParams {
    readonly entryId: number;
    readonly request: UpdateEntryRequest;
}

interface UseUpdateDraftEntryMutationOptions {
    readonly onSuccess?: (data: EntryResponse) => void;
    readonly onError?: (error: ApiError) => void;
}

export function useUpdateDraftEntryMutation(
    options?: UseUpdateDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId, request }: UpdateEntryParams) =>
            draftsApi.updateDraftEntry(entryId, request),
        onSuccess: (data) => {
            void queryClient.invalidateQueries({
                queryKey: ["drafts"],
            });
            void queryClient.invalidateQueries({
                queryKey: ["drafts", "detail", data.id],
            });
            void queryClient.invalidateQueries({
                queryKey: ["collections-hierarchy"],
            });
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
