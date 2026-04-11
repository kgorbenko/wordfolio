import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactNode } from "react";

import { useRegisterMutation } from "../../../../src/features/auth/hooks/useRegisterMutation";
import * as authApi from "../../../../src/features/auth/api/authApi";

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
        const registerSpy = vi
            .spyOn(authApi.authApi, "register")
            .mockResolvedValue(undefined);
        const loginSpy = vi
            .spyOn(authApi.authApi, "login")
            .mockResolvedValue(mockLoginResponse);

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

        expect(registerSpy).toHaveBeenCalledWith({
            email: credentials.email,
            password: credentials.password,
        });
        expect(loginSpy).toHaveBeenCalledWith({
            email: credentials.email,
            password: credentials.password,
        });
        expect(onSuccess).toHaveBeenCalledWith(expectedTokens);
        expect(result.current.data).toEqual(expectedTokens);
    });

    it("propagates register failure and does not call login", async () => {
        const registerError = { status: 400, errors: ["Email already taken"] };
        vi.spyOn(authApi.authApi, "register").mockRejectedValue(registerError);
        const loginSpy = vi.spyOn(authApi.authApi, "login");

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

        expect(loginSpy).not.toHaveBeenCalled();
        expect(onError.mock.calls[0][0]).toEqual(registerError);
    });

    it("propagates login failure after successful register", async () => {
        vi.spyOn(authApi.authApi, "register").mockResolvedValue(undefined);
        const loginError = { status: 401, errors: ["Unauthorized"] };
        vi.spyOn(authApi.authApi, "login").mockRejectedValue(loginError);

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

        expect(onSuccess).not.toHaveBeenCalled();
        expect(onError.mock.calls[0][0]).toEqual(loginError);
    });
});
