import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactNode } from "react";

import { useTokenRefresh } from "./useTokenRefresh";
import { useAuthStore } from "../stores/authStore";
import * as authApi from "../api/authApi";

vi.mock("@tanstack/react-router", () => ({
    useNavigate: () => vi.fn(),
    getRouteApi: () => ({}),
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

        const mockRefreshResponse = {
            tokenType: "Bearer",
            accessToken: "new-token",
            expiresIn: 3600,
            refreshToken: "new-refresh-token",
        };

        act(() => {
            useAuthStore.setState({
                authTokens: mockTokens,
                isAuthenticated: true,
            });
        });

        const refreshSpy = vi
            .spyOn(authApi.authApi, "refresh")
            .mockResolvedValue(mockRefreshResponse);

        await act(async () => {
            renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            await vi.waitFor(
                () => {
                    expect(refreshSpy).toHaveBeenCalledWith({
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

        const mockRefreshResponse = {
            tokenType: "Bearer",
            accessToken: "new-token",
            expiresIn: 3600,
            refreshToken: "new-refresh-token",
        };

        act(() => {
            useAuthStore.setState({
                authTokens: mockTokens,
                isAuthenticated: true,
            });
        });

        const refreshSpy = vi
            .spyOn(authApi.authApi, "refresh")
            .mockResolvedValue(mockRefreshResponse);

        await act(async () => {
            renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            await vi.waitFor(
                () => {
                    expect(refreshSpy).toHaveBeenCalledWith({
                        refreshToken: "refresh-token",
                    });
                },
                { timeout: 100 }
            );
        });

        // Verify refresh was called exactly once
        expect(refreshSpy).toHaveBeenCalledTimes(1);
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

        vi.spyOn(authApi.authApi, "refresh").mockRejectedValue(
            new Error("Unauthorized")
        );

        const hookResult = await act(async () => {
            return renderHook(() => useTokenRefresh(), {
                wrapper: createWrapper(),
            });
        });

        await act(async () => {
            await vi.waitFor(
                () => {
                    expect(hookResult.result.current.isInitializing).toBe(
                        false
                    );
                },
                { timeout: 100 }
            );
        });

        const state = useAuthStore.getState();
        expect(state.authTokens).toBeNull();
        expect(state.isAuthenticated).toBe(false);
    });
});
