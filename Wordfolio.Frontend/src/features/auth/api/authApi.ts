import { useAuthStore } from "../../../shared/stores/authStore";

export interface RegisterRequest {
    readonly email: string;
    readonly password: string;
}

export interface LoginRequest {
    readonly email: string;
    readonly password: string;
}

export interface LoginResponse {
    readonly tokenType: string;
    readonly accessToken: string;
    readonly expiresIn: number;
    readonly refreshToken: string;
}

export interface RefreshRequest {
    readonly refreshToken: string;
}

export interface PasswordRequirements {
    readonly requiredLength: number;
    readonly requireDigit: boolean;
    readonly requireLowercase: boolean;
    readonly requireUppercase: boolean;
    readonly requireNonAlphanumeric: boolean;
    readonly requiredUniqueChars: number;
}

export interface UserInfoResponse {
    readonly email: string;
    readonly isEmailConfirmed: boolean;
}

const API_BASE_URL = "/api";

const getAuthHeaders = (): HeadersInit => {
    const authTokens = useAuthStore.getState().authTokens;
    if (!authTokens) {
        throw new Error("Not authenticated");
    }
    return {
        Authorization: `${authTokens.tokenType} ${authTokens.accessToken}`,
    };
};

export const authApi = {
    register: async (request: RegisterRequest): Promise<void> => {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            throw await response.json();
        }
    },

    login: async (request: LoginRequest): Promise<LoginResponse> => {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    getPasswordRequirements: async (): Promise<PasswordRequirements> => {
        const response = await fetch(
            `${API_BASE_URL}/auth/password-requirements`,
            {
                method: "GET",
            }
        );

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    refresh: async (request: RefreshRequest): Promise<LoginResponse> => {
        const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    getUserInfo: async (): Promise<UserInfoResponse> => {
        const response = await fetch(`${API_BASE_URL}/auth/manage/info`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },
};
