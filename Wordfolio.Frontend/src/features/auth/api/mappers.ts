import type {
    LoginRequest,
    LoginResponse,
    PasswordRequirements as PasswordRequirementsResponse,
    RefreshRequest,
    RegisterRequest,
} from "./authApi";
import type {
    AuthTokens,
    LoginCredentials,
    PasswordRequirements,
    RefreshCredentials,
    RegisterCredentials,
} from "../types";

export function mapPasswordRequirements(
    response: PasswordRequirementsResponse
): PasswordRequirements {
    return {
        requiredLength: response.requiredLength,
        requireDigit: response.requireDigit,
        requireLowercase: response.requireLowercase,
        requireUppercase: response.requireUppercase,
        requireNonAlphanumeric: response.requireNonAlphanumeric,
        requiredUniqueChars: response.requiredUniqueChars,
    };
}

export function mapLoginRequest(credentials: LoginCredentials): LoginRequest {
    return { email: credentials.email, password: credentials.password };
}

export function mapRegisterRequest(
    credentials: RegisterCredentials
): RegisterRequest {
    return { email: credentials.email, password: credentials.password };
}

export function mapRefreshRequest(
    credentials: RefreshCredentials
): RefreshRequest {
    return { refreshToken: credentials.refreshToken };
}

export function mapAuthTokens(response: LoginResponse): AuthTokens {
    return {
        tokenType: response.tokenType,
        accessToken: response.accessToken,
        expiresIn: response.expiresIn,
        refreshToken: response.refreshToken,
    };
}
