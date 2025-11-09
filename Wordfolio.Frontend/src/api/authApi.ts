export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}

export interface ApiError {
  type?: string;
  title?: string;
  status?: number;
  errors?: Record<string, string[]>;
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
