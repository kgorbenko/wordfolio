import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AuthTokens {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}

interface AuthState {
  tokens: AuthTokens | null;
  isAuthenticated: boolean;
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
      name: 'auth-storage',
    }
  )
);
