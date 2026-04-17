import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate, useRouter } from "@tanstack/react-router";

import { useRefreshMutation } from "../../../shared/api/mutations/auth";
import { useAuthStore } from "../../../shared/stores/authStore";
import { getSafeRedirectPath } from "../../../shared/utils/redirectUtils";
import { loginPath } from "../routes";

const REFRESH_BUFFER_MS = 5 * 60 * 1000;

export const useTokenRefresh = () => {
    const { authTokens, setTokens, clearAuth } = useAuthStore();
    const navigate = useNavigate();
    const router = useRouter();
    const [isInitializing, setIsInitializing] = useState(true);
    const refreshTimeoutRef = useRef<number | null>(null);
    const hasInitializedRef = useRef(false);

    const refreshMutation = useRefreshMutation({
        onSuccess: async (data) => {
            setTokens(data);
            setIsInitializing(false);
        },
        onError: () => {
            clearAuth();
            setIsInitializing(false);
            const safeRedirect = getSafeRedirectPath(
                router.state.location.href
            );
            navigate({
                ...loginPath(),
                search: {
                    message: "Your session has expired. Please log in again.",
                    ...(safeRedirect !== undefined && {
                        redirect: safeRedirect,
                    }),
                },
                replace: true,
            });
        },
    });

    const scheduleTokenRefresh = useCallback(
        (expiresIn: number) => {
            if (refreshTimeoutRef.current !== null) {
                clearTimeout(refreshTimeoutRef.current);
            }

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

    useEffect(() => {
        if (hasInitializedRef.current) {
            return;
        }

        hasInitializedRef.current = true;

        if (authTokens?.refreshToken && !refreshMutation.isPending) {
            refreshMutation.mutate({ refreshToken: authTokens.refreshToken });
        } else {
            setIsInitializing(false);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

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
