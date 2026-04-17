import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactNode } from "react";

import { useTokenRefresh } from "../../../../src/features/auth/hooks/useTokenRefresh";
import { useAuthStore } from "../../../../src/shared/stores/authStore";

const mockNavigate = vi.fn();
const mockRefreshMutate = vi.fn();
let mockRefreshIsPending = false;
let mockRefreshOnError: (() => void) | undefined;
let mockRouterHref = "/";

vi.mock("@tanstack/react-router", () => ({
    useNavigate: () => mockNavigate,
    useRouter: () => ({ state: { location: { href: mockRouterHref } } }),
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
        vi.unstubAllGlobals();
        mockRefreshIsPending = false;
        mockRefreshOnError = undefined;
        mockRouterHref = "/";
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

    it("should navigate to login with redirect when refresh fails on a protected page", async () => {
        mockRouterHref = "/dashboard";

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

        await act(async () => {
            renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            mockRefreshOnError?.();
        });

        expect(mockNavigate).toHaveBeenCalledWith({
            to: "/login",
            search: {
                message: "Your session has expired. Please log in again.",
                redirect: "/dashboard",
            },
            replace: true,
        });
    });

    it("should navigate to login without redirect when refresh fails on the login page", async () => {
        mockRouterHref = "/login";

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

        await act(async () => {
            renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            mockRefreshOnError?.();
        });

        expect(mockNavigate).toHaveBeenCalledWith({
            to: "/login",
            search: {
                message: "Your session has expired. Please log in again.",
            },
            replace: true,
        });
    });

    describe("timer scheduling", () => {
        beforeEach(() => {
            vi.useFakeTimers();
        });

        afterEach(() => {
            vi.useRealTimers();
        });

        it("should not refresh immediately for short TTL tokens (no tight-loop)", async () => {
            const now = 1_000_000_000_000;
            vi.setSystemTime(now);

            const { result } = renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });

            expect(result.current.isInitializing).toBe(false);

            act(() => {
                useAuthStore.setState({
                    authTokens: {
                        tokenType: "Bearer",
                        accessToken: "access-token",
                        expiresIn: 10,
                        refreshToken: "refresh-token",
                        setAt: now,
                    },
                    isAuthenticated: true,
                });
            });

            act(() => {
                vi.advanceTimersByTime(4999);
            });
            expect(mockRefreshMutate).not.toHaveBeenCalled();

            act(() => {
                vi.advanceTimersByTime(1);
            });
            expect(mockRefreshMutate).toHaveBeenCalledTimes(1);
        });

        it("should refresh 5 minutes before expiry for normal TTL tokens", async () => {
            const now = 1_000_000_000_000;
            vi.setSystemTime(now);

            const { result } = renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });

            expect(result.current.isInitializing).toBe(false);

            act(() => {
                useAuthStore.setState({
                    authTokens: {
                        tokenType: "Bearer",
                        accessToken: "access-token",
                        expiresIn: 3600,
                        refreshToken: "refresh-token",
                        setAt: now,
                    },
                    isAuthenticated: true,
                });
            });

            act(() => {
                vi.advanceTimersByTime(3_299_999);
            });
            expect(mockRefreshMutate).not.toHaveBeenCalled();

            act(() => {
                vi.advanceTimersByTime(1);
            });
            expect(mockRefreshMutate).toHaveBeenCalledTimes(1);
        });

        it("should use remaining lifetime for persisted tokens based on setAt", async () => {
            const now = 1_000_000_000_000;
            vi.setSystemTime(now);

            const { result } = renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });

            expect(result.current.isInitializing).toBe(false);

            act(() => {
                useAuthStore.setState({
                    authTokens: {
                        tokenType: "Bearer",
                        accessToken: "access-token",
                        expiresIn: 3600,
                        refreshToken: "refresh-token",
                        setAt: now - 30 * 60 * 1000,
                    },
                    isAuthenticated: true,
                });
            });

            act(() => {
                vi.advanceTimersByTime(1_499_999);
            });
            expect(mockRefreshMutate).not.toHaveBeenCalled();

            act(() => {
                vi.advanceTimersByTime(1);
            });
            expect(mockRefreshMutate).toHaveBeenCalledTimes(1);
        });

        it("should enforce minimum 1000ms delay for expired tokens to prevent tight-loop", async () => {
            const now = 1_000_000_000_000;
            vi.setSystemTime(now);

            const { result } = renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });

            expect(result.current.isInitializing).toBe(false);

            act(() => {
                useAuthStore.setState({
                    authTokens: {
                        tokenType: "Bearer",
                        accessToken: "access-token",
                        expiresIn: 5,
                        refreshToken: "refresh-token",
                        setAt: now - 10_000,
                    },
                    isAuthenticated: true,
                });
            });

            act(() => {
                vi.advanceTimersByTime(999);
            });
            expect(mockRefreshMutate).not.toHaveBeenCalled();

            act(() => {
                vi.advanceTimersByTime(1);
            });
            expect(mockRefreshMutate).toHaveBeenCalledTimes(1);

            act(() => {
                vi.setSystemTime(now + 1000);
                useAuthStore.setState({
                    authTokens: {
                        tokenType: "Bearer",
                        accessToken: "new-access-token",
                        expiresIn: 5,
                        refreshToken: "new-refresh-token",
                        setAt: now + 1000,
                    },
                    isAuthenticated: true,
                });
            });

            vi.clearAllMocks();

            act(() => {
                vi.advanceTimersByTime(999);
            });
            expect(mockRefreshMutate).not.toHaveBeenCalled();
        });

        it("should clear pending timer on unmount", async () => {
            const now = 1_000_000_000_000;
            vi.setSystemTime(now);

            const { result, unmount } = renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });

            expect(result.current.isInitializing).toBe(false);

            act(() => {
                useAuthStore.setState({
                    authTokens: {
                        tokenType: "Bearer",
                        accessToken: "access-token",
                        expiresIn: 3600,
                        refreshToken: "refresh-token",
                        setAt: now,
                    },
                    isAuthenticated: true,
                });
            });

            unmount();

            act(() => {
                vi.advanceTimersByTime(3_300_000);
            });
            expect(mockRefreshMutate).not.toHaveBeenCalled();
        });
    });
});
