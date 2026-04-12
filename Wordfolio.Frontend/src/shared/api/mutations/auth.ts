import { useMutation, useQueryClient } from "@tanstack/react-query";

import { client } from "../client";
import {
    mapAuthTokens,
    mapLoginRequest,
    mapRefreshRequest,
    mapRegisterRequest,
} from "../mappers/auth";
import type {
    AuthTokens,
    LoginCredentials,
    RefreshCredentials,
    RegisterCredentials,
} from "../types/auth";
import type { ApiError } from "../types/entries";

interface UseLoginMutationOptions {
    readonly onSuccess?: (data: AuthTokens) => Promise<void>;
    readonly onError?: (error: ApiError) => void;
}

export function useLoginMutation(options?: UseLoginMutationOptions) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (credentials: LoginCredentials) => {
            const { data, error } = await client.POST("/auth/login", {
                body: mapLoginRequest(credentials),
            });
            if (error) throw error;
            return mapAuthTokens(data!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}

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
            const { error: registerError } = await client.POST(
                "/auth/register",
                {
                    body: mapRegisterRequest(credentials),
                }
            );
            if (registerError) throw registerError;

            const { data, error } = await client.POST("/auth/login", {
                body: mapLoginRequest(credentials),
            });
            if (error) throw error;

            return mapAuthTokens(data!);
        },
        onSuccess: async (data) => {
            await options?.onSuccess?.(data);
            void queryClient.invalidateQueries();
        },
        onError: options?.onError,
    });
}

interface UseRefreshMutationOptions {
    readonly onSuccess?: (data: AuthTokens) => Promise<void>;
    readonly onError?: () => void;
}

export const useRefreshMutation = (options?: UseRefreshMutationOptions) => {
    return useMutation({
        mutationFn: async (credentials: RefreshCredentials) => {
            const { data, error } = await client.POST("/auth/refresh", {
                body: mapRefreshRequest(credentials),
            });
            if (error) throw error;
            return mapAuthTokens(data!);
        },
        onSuccess: options?.onSuccess,
        onError: options?.onError,
    });
};
