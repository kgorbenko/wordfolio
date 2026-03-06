import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { ApiError } from "../../../shared/api/common";
import { authApi } from "../api/authApi";
import { mapAuthTokens, mapLoginRequest } from "../api/mappers";
import type { AuthTokens, LoginCredentials } from "../types";

interface UseLoginMutationOptions {
    onSuccess?: (data: AuthTokens) => void;
    onError?: (error: ApiError) => void;
}

export function useLoginMutation(options?: UseLoginMutationOptions) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (credentials: LoginCredentials) => {
            const response = await authApi.login(mapLoginRequest(credentials));
            return mapAuthTokens(response);
        },
        onSuccess: (data) => {
            void queryClient.invalidateQueries();
            options?.onSuccess?.(data);
        },
        onError: options?.onError,
    });
}
