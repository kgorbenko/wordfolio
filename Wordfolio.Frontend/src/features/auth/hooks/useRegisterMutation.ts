import { useMutation, useQueryClient } from "@tanstack/react-query";

import type { ApiError } from "../../../shared/api/types/entries";
import { authApi } from "../api/authApi";
import {
    mapAuthTokens,
    mapLoginRequest,
    mapRegisterRequest,
} from "../api/mappers";
import type { AuthTokens, RegisterCredentials } from "../types";

interface UseRegisterMutationOptions {
    readonly onSuccess?: (data: AuthTokens) => Promise<void>;
    readonly onError?: (error: ApiError) => void;
}

export function useRegisterMutation(options?: UseRegisterMutationOptions) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (
            credentials: RegisterCredentials
        ): Promise<AuthTokens> => {
            await authApi.register(mapRegisterRequest(credentials));
            const loginResponse = await authApi.login(
                mapLoginRequest(credentials)
            );
            return mapAuthTokens(loginResponse);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}
