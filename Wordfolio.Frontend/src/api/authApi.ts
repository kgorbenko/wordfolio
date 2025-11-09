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

export interface ApiError {
  readonly type?: string;
  readonly title?: string;
  readonly status?: number;
  readonly errors?: Record<string, string[]>;
}

const API_BASE_URL = '/api';

export const authApi = {
  register: async (request: RegisterRequest): Promise<void> => {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error: ApiError = await response.json();
      throw error;
    }
  },

  login: async (request: LoginRequest): Promise<LoginResponse> => {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error: ApiError = await response.json();
      throw error;
    }

    return response.json();
  },
};
