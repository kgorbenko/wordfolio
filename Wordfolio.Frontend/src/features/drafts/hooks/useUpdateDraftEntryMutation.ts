import { useMutation, useQueryClient } from "@tanstack/react-query";

import { mapEntry, mapUpdateEntryData } from "../../../shared/api/entryMappers";
import { draftsApi } from "../api/draftsApi";
import type { CreateEntryData, Entry } from "../../../shared/types/entries";

interface UpdateEntryParams {
    readonly entryId: number;
    readonly data: CreateEntryData;
}

interface UseUpdateDraftEntryMutationOptions {
    readonly onSuccess?: (data: Entry) => void;
    readonly onError?: () => void;
}

export function useUpdateDraftEntryMutation(
    options?: UseUpdateDraftEntryMutationOptions
) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ entryId, data }: UpdateEntryParams) =>
            draftsApi.updateDraftEntry(entryId, mapUpdateEntryData(data)),
        onSuccess: (data) => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.(mapEntry(data));
        },
        onError: options?.onError,
    });
}
