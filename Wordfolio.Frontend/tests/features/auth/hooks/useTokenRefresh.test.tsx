import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactNode } from "react";

import { useTokenRefresh } from "../../../../src/features/auth/hooks/useTokenRefresh";
import { useAuthStore } from "../../../../src/shared/stores/authStore";

const mockRefreshMutate = vi.fn();
let mockRefreshIsPending = false;
let mockRefreshOnError: (() => void) | undefined;

vi.mock("@tanstack/react-router", () => ({
    useNavigate: () => vi.fn(),
    getRouteApi: () => ({}),
}));

vi.mock("../../../../src/shared/api/mutations/auth", () => ({
    useRefreshMutation: (options?: { onError?: () => void }) => {
        mockRefreshOnError = options?.onError;
        return {
            mutate: mockRefreshMutate,
            isPending: mockRefreshIsPending,
        };
    },
}));

const createWrapper = () => {
    const queryClient = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });

    return ({ children }: { children: ReactNode }) => (
        <QueryClientProvider client={queryClient}>
            {children}
        </QueryClientProvider>
    );
};

describe("useTokenRefresh", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockRefreshIsPending = false;
        mockRefreshOnError = undefined;
        act(() => {
            useAuthStore.setState({
                authTokens: null,
                isAuthenticated: false,
            });
        });
    });

    it("should return isInitializing state", () => {
        const { result } = renderHook(() => useTokenRefresh(), {
            wrapper: createWrapper(),
        });

        expect(result.current).toHaveProperty("isInitializing");
        expect(typeof result.current.isInitializing).toBe("boolean");
    });

    it("should set isInitializing to false when no tokens exist", async () => {
        const { result } = renderHook(() => useTokenRefresh(), {
            wrapper: createWrapper(),
        });

        await vi.waitFor(
            () => {
                expect(result.current.isInitializing).toBe(false);
            },
            { timeout: 100 }
        );
    });

    it("should call refresh API when tokens exist on mount", async () => {
        const mockTokens = {
            tokenType: "Bearer",
            accessToken: "old-token",
            expiresIn: 3600,
            refreshToken: "refresh-token",
            setAt: Date.now(),
        };

        act(() => {
            useAuthStore.setState({
                authTokens: mockTokens,
                isAuthenticated: true,
            });
        });

        await act(async () => {
            renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            await vi.waitFor(
                () => {
                    expect(mockRefreshMutate).toHaveBeenCalledWith({
                        refreshToken: "refresh-token",
                    });
                },
                { timeout: 100 }
            );
        });
    });

    it("should call refresh API only once on mount even with StrictMode", async () => {
        const mockTokens = {
            tokenType: "Bearer",
            accessToken: "old-token",
            expiresIn: 3600,
            refreshToken: "refresh-token",
            setAt: Date.now(),
        };

        act(() => {
            useAuthStore.setState({
                authTokens: mockTokens,
                isAuthenticated: true,
            });
        });

        await act(async () => {
            renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            await vi.waitFor(
                () => {
                    expect(mockRefreshMutate).toHaveBeenCalledWith({
                        refreshToken: "refresh-token",
                    });
                },
                { timeout: 100 }
            );
        });

        // Verify refresh was called exactly once
        expect(mockRefreshMutate).toHaveBeenCalledTimes(1);
    });

    it("should clear auth state when refresh fails", async () => {
        const mockTokens = {
            tokenType: "Bearer",
            accessToken: "old-token",
            expiresIn: 3600,
            refreshToken: "invalid-token",
            setAt: Date.now(),
        };

        act(() => {
            useAuthStore.setState({
                authTokens: mockTokens,
                isAuthenticated: true,
            });
        });

        const hookResult = await act(async () => {
            return renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            mockRefreshOnError?.();
        });

        await vi.waitFor(
            () => {
                expect(hookResult.result.current.isInitializing).toBe(false);
            },
            { timeout: 100 }
        );

        const state = useAuthStore.getState();
        expect(state.authTokens).toBeNull();
        expect(state.isAuthenticated).toBe(false);
    });
});
