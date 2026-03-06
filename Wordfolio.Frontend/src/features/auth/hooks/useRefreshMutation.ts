import { useMutation } from "@tanstack/react-query";

import { authApi } from "../api/authApi";
import { mapAuthTokens, mapRefreshRequest } from "../api/mappers";
import type { AuthTokens, RefreshCredentials } from "../types";

interface UseRefreshMutationOptions {
    onSuccess?: (data: AuthTokens) => void;
    onError?: () => void;
}

export const useRefreshMutation = (options?: UseRefreshMutationOptions) => {
    return useMutation({
        mutationFn: async (credentials: RefreshCredentials) => {
            const response = await authApi.refresh(
                mapRefreshRequest(credentials)
            );
            return mapAuthTokens(response);
        },
        onSuccess: options?.onSuccess,
        onError: options?.onError,
    });
};
