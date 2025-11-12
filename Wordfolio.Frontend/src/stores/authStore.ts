import { create } from "zustand";
import { persist } from "zustand/middleware";

interface AuthTokens {
    readonly tokenType: string;
    readonly accessToken: string;
    readonly expiresIn: number;
    readonly refreshToken: string;
}

interface AuthState {
    readonly tokens: AuthTokens | null;
    readonly isAuthenticated: boolean;
    setTokens: (tokens: AuthTokens) => void;
    clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            tokens: null,
            isAuthenticated: false,
            setTokens: (tokens: AuthTokens) =>
                set({ tokens, isAuthenticated: true }),
            clearAuth: () => set({ tokens: null, isAuthenticated: false }),
        }),
        {
            name: "auth-storage",
        }
    )
);
