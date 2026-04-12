import type { components } from "../../../shared/api/generated/schema";
import type {
    AuthTokens,
    LoginCredentials,
    PasswordRequirements,
    RefreshCredentials,
    RegisterCredentials,
    UserInfo,
} from "../types";

type LoginRequest = components["schemas"]["LoginRequest"];
type RegisterRequest = components["schemas"]["RegisterRequest"];
type RefreshRequest = components["schemas"]["RefreshRequest"];
type AccessTokenResponse = components["schemas"]["AccessTokenResponse"];
type PasswordRequirementsResponse =
    components["schemas"]["PasswordRequirementsResponse"];
type InfoResponse = components["schemas"]["InfoResponse"];

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

export function mapAuthTokens(response: AccessTokenResponse): AuthTokens {
    return {
        tokenType: response.tokenType ?? "",
        accessToken: response.accessToken,
        expiresIn: response.expiresIn,
        refreshToken: response.refreshToken,
    };
}

export function mapUserInfo(response: InfoResponse): UserInfo {
    return {
        email: response.email,
        isEmailConfirmed: response.isEmailConfirmed,
    };
}
