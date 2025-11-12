import { useCallback, useEffect, useRef, useState } from "react";

import { useAuthStore } from "../stores/authStore";
import { useRefreshMutation } from "../mutations/useRefreshMutation";

// Refresh token 5 minutes before it expires
const REFRESH_BUFFER_MS = 5 * 60 * 1000;

export const useTokenRefresh = () => {
    const { tokens, tokenSetAt, setTokens, clearAuth } = useAuthStore();
    const [isInitialRefreshing, setIsInitialRefreshing] = useState(false);
    const [hasAttemptedInitialRefresh, setHasAttemptedInitialRefresh] =
        useState(false);
    const refreshTimeoutRef = useRef<number | null>(null);

    const refreshMutation = useRefreshMutation({
        onSuccess: (data) => {
            setTokens(data);
            setIsInitialRefreshing(false);
        },
        onError: () => {
            clearAuth();
            setIsInitialRefreshing(false);
        },
    });

    const scheduleTokenRefresh = useCallback(
        (expiresIn: number) => {
            // Clear any existing timeout
            if (refreshTimeoutRef.current !== null) {
                clearTimeout(refreshTimeoutRef.current);
            }

            // Calculate when to refresh (expiresIn is in seconds)
            const refreshInMs = Math.max(
                expiresIn * 1000 - REFRESH_BUFFER_MS,
                0
            );

            refreshTimeoutRef.current = window.setTimeout(() => {
                if (tokens?.refreshToken) {
                    refreshMutation.mutate({
                        refreshToken: tokens.refreshToken,
                    });
                }
            }, refreshInMs);
        },
        [tokens, refreshMutation]
    );

    // Attempt to refresh on startup if we have a refresh token
    useEffect(() => {
        if (
            !hasAttemptedInitialRefresh &&
            tokens?.refreshToken &&
            !refreshMutation.isPending
        ) {
            setHasAttemptedInitialRefresh(true);
            setIsInitialRefreshing(true);
            refreshMutation.mutate({ refreshToken: tokens.refreshToken });
        }
    }, [
        tokens,
        hasAttemptedInitialRefresh,
        refreshMutation.isPending,
        refreshMutation,
    ]);

    // Schedule background refresh based on expiresIn
    useEffect(() => {
        if (tokens && tokenSetAt && !isInitialRefreshing) {
            scheduleTokenRefresh(tokens.expiresIn);
        }

        return () => {
            if (refreshTimeoutRef.current !== null) {
                clearTimeout(refreshTimeoutRef.current);
            }
        };
    }, [tokens, tokenSetAt, isInitialRefreshing, scheduleTokenRefresh]);

    return {
        isInitialRefreshing,
        hasAttemptedInitialRefresh,
    };
};
