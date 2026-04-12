import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";

import { useRegisterMutation } from "../../../../src/shared/api/mutations/auth";

const { mockPost } = vi.hoisted(() => ({
    mockPost: vi.fn(),
}));

vi.mock("../../../../src/shared/api/client", () => ({
    client: {
        POST: mockPost,
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

const mockLoginResponse = {
    tokenType: "Bearer",
    accessToken: "access-token",
    expiresIn: 3600,
    refreshToken: "refresh-token",
};

const expectedTokens = {
    tokenType: "Bearer",
    accessToken: "access-token",
    expiresIn: 3600,
    refreshToken: "refresh-token",
};

const credentials = { email: "user@example.com", password: "Password1!" };

describe("useRegisterMutation", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("calls register then login on success and returns tokens", async () => {
        mockPost
            .mockResolvedValueOnce({ error: undefined })
            .mockResolvedValueOnce({
                data: mockLoginResponse,
                error: undefined,
            });

        const onSuccess = vi.fn().mockResolvedValue(undefined);

        const { result } = renderHook(
            () => useRegisterMutation({ onSuccess }),
            { wrapper: createWrapper() }
        );

        await act(async () => {
            result.current.mutate(credentials);
        });

        await vi.waitFor(() => {
            expect(result.current.isSuccess).toBe(true);
        });

        expect(mockPost).toHaveBeenNthCalledWith(1, "/auth/register", {
            body: credentials,
        });
        expect(mockPost).toHaveBeenNthCalledWith(2, "/auth/login", {
            body: credentials,
        });
        expect(onSuccess).toHaveBeenCalledWith(expectedTokens);
        expect(result.current.data).toEqual(expectedTokens);
    });

    it("propagates register failure and does not call login", async () => {
        const registerError = { status: 400, errors: ["Email already taken"] };
        mockPost.mockResolvedValueOnce({ error: registerError });

        const onError = vi.fn();

        const { result } = renderHook(() => useRegisterMutation({ onError }), {
            wrapper: createWrapper(),
        });

        await act(async () => {
            result.current.mutate(credentials);
        });

        await vi.waitFor(() => {
            expect(result.current.isError).toBe(true);
        });

        expect(mockPost).toHaveBeenCalledTimes(1);
        expect(onError.mock.calls[0][0]).toEqual(registerError);
    });

    it("propagates login failure after successful register", async () => {
        const loginError = { status: 401, errors: ["Unauthorized"] };

        mockPost
            .mockResolvedValueOnce({ error: undefined })
            .mockResolvedValueOnce({ error: loginError });

        const onSuccess = vi.fn();
        const onError = vi.fn();

        const { result } = renderHook(
            () => useRegisterMutation({ onSuccess, onError }),
            { wrapper: createWrapper() }
        );

        await act(async () => {
            result.current.mutate(credentials);
        });

        await vi.waitFor(() => {
            expect(result.current.isError).toBe(true);
        });

        expect(mockPost).toHaveBeenCalledTimes(2);
        expect(onSuccess).not.toHaveBeenCalled();
        expect(onError.mock.calls[0][0]).toEqual(loginError);
    });
});
