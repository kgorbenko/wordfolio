import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";

import { RegisterPage } from "../../../../src/features/auth/pages/RegisterPage";

const mockNavigate = vi.fn();
const mockSetTokens = vi.fn();

const mockPasswordRequirements = {
    requiredLength: 6,
    requireDigit: false,
    requireLowercase: false,
    requireUppercase: false,
    requireNonAlphanumeric: false,
    requiredUniqueChars: 1,
};

const mockTokens = {
    tokenType: "Bearer",
    accessToken: "access-token",
    expiresIn: 3600,
    refreshToken: "refresh-token",
};

let mockSearchParams: { redirect?: string } = {};
let mockRegisterMutate = vi.fn();
let mockRegisterIsPending = false;
let mockRegisterOnSuccess:
    | ((data: typeof mockTokens) => Promise<void>)
    | undefined;

vi.mock("@tanstack/react-router", () => ({
    useNavigate: () => mockNavigate,
    getRouteApi: () => ({
        useSearch: () => mockSearchParams,
    }),
    Link: ({
        children,
        to,
        search,
    }: {
        to: string;
        search?: Record<string, string>;
        children: ReactNode;
    }) => (
        <a
            href={
                to +
                (search ? "?" + new URLSearchParams(search).toString() : "")
            }
        >
            {children}
        </a>
    ),
}));

vi.mock("../../../../src/features/auth/hooks/useRegisterMutation", () => ({
    useRegisterMutation: (options?: {
        onSuccess?: (data: typeof mockTokens) => Promise<void>;
    }) => {
        mockRegisterOnSuccess = options?.onSuccess;
        return {
            mutate: mockRegisterMutate,
            isPending: mockRegisterIsPending,
        };
    },
}));

vi.mock(
    "../../../../src/features/auth/hooks/usePasswordRequirementsQuery",
    () => ({
        usePasswordRequirementsQuery: () => ({
            data: mockPasswordRequirements,
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        }),
    })
);

vi.mock("../../../../src/shared/stores/authStore", () => ({
    useAuthStore: vi.fn(
        (selector: (state: { setTokens: typeof mockSetTokens }) => unknown) =>
            selector({ setTokens: mockSetTokens })
    ),
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

describe("RegisterPage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockSearchParams = {};
        mockRegisterMutate = vi.fn();
        mockRegisterIsPending = false;
        mockRegisterOnSuccess = undefined;
    });

    it("stores tokens and navigates home when no redirect is present", async () => {
        mockSearchParams = {};
        render(<RegisterPage />, { wrapper: createWrapper() });

        await act(async () => {
            await mockRegisterOnSuccess?.(mockTokens);
        });

        expect(mockSetTokens).toHaveBeenCalledWith(mockTokens);
        expect(mockNavigate).toHaveBeenCalledWith({
            to: "/",
            replace: true,
        });
    });

    it("stores tokens and navigates to redirect destination on success", async () => {
        mockSearchParams = { redirect: "/dashboard" };
        render(<RegisterPage />, { wrapper: createWrapper() });

        await act(async () => {
            await mockRegisterOnSuccess?.(mockTokens);
        });

        expect(mockSetTokens).toHaveBeenCalledWith(mockTokens);
        expect(mockNavigate).toHaveBeenCalledWith({
            to: "/dashboard",
            replace: true,
        });
    });

    it("footer login link preserves redirect query parameter", () => {
        mockSearchParams = { redirect: "/dashboard" };
        render(<RegisterPage />, { wrapper: createWrapper() });

        const loginLink = screen.getByRole("link", { name: /login here/i });
        expect(loginLink.getAttribute("href")).toContain("redirect=");
        expect(loginLink.getAttribute("href")).toContain("dashboard");
    });

    it("footer login link works without redirect parameter", () => {
        mockSearchParams = {};
        render(<RegisterPage />, { wrapper: createWrapper() });

        const loginLink = screen.getByRole("link", { name: /login here/i });
        expect(loginLink.getAttribute("href")).toBe("/login");
    });

    it("submits registration form with entered credentials", async () => {
        const user = userEvent.setup();
        mockSearchParams = {};
        render(<RegisterPage />, { wrapper: createWrapper() });

        await user.type(screen.getByLabelText(/email/i), "test@example.com");
        await user.type(screen.getByLabelText(/^password$/i), "password123");
        await user.type(
            screen.getByLabelText(/confirm password/i),
            "password123"
        );
        await user.click(
            screen.getByRole("button", { name: /create archive/i })
        );

        expect(mockRegisterMutate).toHaveBeenCalledWith({
            email: "test@example.com",
            password: "password123",
        });
    });
});
