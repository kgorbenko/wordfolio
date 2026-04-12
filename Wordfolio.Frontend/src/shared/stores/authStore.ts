import { create } from "zustand";
import { persist } from "zustand/middleware";

import type { AuthTokens } from "../api/types/auth";

interface StoredAuthTokens extends AuthTokens {
    readonly setAt: number;
}

interface AuthState {
    readonly authTokens: StoredAuthTokens | null;
    readonly isAuthenticated: boolean;
    setTokens: (tokens: AuthTokens) => void;
    clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            authTokens: null,
            isAuthenticated: false,
            setTokens: (tokens: AuthTokens) =>
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
