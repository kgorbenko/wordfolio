import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "@tanstack/react-router";

import { useAuthStore } from "../stores/authStore";
import { useRefreshMutation } from "../mutations/useRefreshMutation";

// Refresh token 5 minutes before it expires
const REFRESH_BUFFER_MS = 5 * 60 * 1000;

export const useTokenRefresh = () => {
    const { authTokens, setTokens, clearAuth } = useAuthStore();
    const navigate = useNavigate();
    const [isInitializing, setIsInitializing] = useState(true);
    const refreshTimeoutRef = useRef<number | null>(null);

    const refreshMutation = useRefreshMutation({
        onSuccess: (data) => {
            setTokens(data);
            setIsInitializing(false);
        },
        onError: () => {
            clearAuth();
            setIsInitializing(false);
            navigate({
                to: "/login",
                search: {
                    message: "Your session has expired. Please log in again.",
                },
            });
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
                if (authTokens?.refreshToken) {
                    refreshMutation.mutate({
                        refreshToken: authTokens.refreshToken,
                    });
                }
            }, refreshInMs);
        },
        [authTokens, refreshMutation]
    );

    // Attempt to refresh on startup if we have a refresh token
    useEffect(() => {
        if (authTokens?.refreshToken && !refreshMutation.isPending) {
            refreshMutation.mutate({ refreshToken: authTokens.refreshToken });
        } else {
            setIsInitializing(false);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Empty deps - only run once on mount

    // Schedule background refresh based on expiresIn
    useEffect(() => {
        if (authTokens && !isInitializing) {
            scheduleTokenRefresh(authTokens.expiresIn);
        }

        return () => {
            if (refreshTimeoutRef.current !== null) {
                clearTimeout(refreshTimeoutRef.current);
            }
        };
    }, [authTokens, isInitializing, scheduleTokenRefresh]);

    return {
        isInitializing,
    };
};
