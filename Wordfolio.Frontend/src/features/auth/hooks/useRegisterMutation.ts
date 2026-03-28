import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { ApiError } from "../../../shared/api/types/entries";
import { authApi } from "../api/authApi";
import { mapRegisterRequest } from "../api/mappers";
import type { RegisterCredentials } from "../types";

interface UseRegisterMutationOptions {
    readonly onSuccess?: () => Promise<void>;
    readonly onError?: (error: ApiError) => void;
}

export function useRegisterMutation(options?: UseRegisterMutationOptions) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (credentials: RegisterCredentials) => {
            await authApi.register(mapRegisterRequest(credentials));
        },
        onSuccess: async () => {
            await options?.onSuccess?.();
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
