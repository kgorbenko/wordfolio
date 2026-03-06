import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { ApiError } from "../../../shared/api/common";
import { authApi } from "../api/authApi";
import { mapRegisterRequest } from "../api/mappers";
import type { RegisterCredentials } from "../types";

interface UseRegisterMutationOptions {
    readonly onSuccess?: () => void;
    readonly onError?: (error: ApiError) => void;
}

export function useRegisterMutation(options?: UseRegisterMutationOptions) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (credentials: RegisterCredentials) => {
            await authApi.register(mapRegisterRequest(credentials));
        },
        onSuccess: () => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.();
        },
        onError: options?.onError,
    });
}
