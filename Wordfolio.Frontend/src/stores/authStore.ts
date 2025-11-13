import { create } from "zustand";
import { persist } from "zustand/middleware";

interface AuthTokens {
    readonly tokenType: string;
    readonly accessToken: string;
    readonly expiresIn: number;
    readonly refreshToken: string;
    readonly setAt: number;
}

interface AuthState {
    readonly authTokens: AuthTokens | null;
    readonly isAuthenticated: boolean;
    setTokens: (tokens: Omit<AuthTokens, "setAt">) => void;
    clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            authTokens: null,
            isAuthenticated: false,
            setTokens: (tokens: Omit<AuthTokens, "setAt">) =>
                set({
                    authTokens: {
                        ...tokens,
                        setAt: Date.now(),
                    },
                    isAuthenticated: true,
                }),
            clearAuth: () => set({ authTokens: null, isAuthenticated: false }),
        }),
        {
            name: "auth-storage",
        }
    )
);
