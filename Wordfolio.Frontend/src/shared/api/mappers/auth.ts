import type { components } from "../generated/schema";
import type {
    AuthTokens,
    LoginCredentials,
    PasswordRequirements,
    RefreshCredentials,
    RegisterCredentials,
    UserInfo,
} from "../types/auth";

type LoginRequest = components["schemas"]["LoginRequest"];
type RegisterRequest = components["schemas"]["RegisterRequest"];
type RefreshRequest = components["schemas"]["RefreshRequest"];
type AccessTokenResponse = components["schemas"]["AccessTokenResponse"];
type PasswordRequirementsResponse =
    components["schemas"]["PasswordRequirementsResponse"];
type InfoResponse = components["schemas"]["InfoResponse"];

export const mapPasswordRequirements = (
    response: PasswordRequirementsResponse
): PasswordRequirements => ({
    requiredLength: response.requiredLength,
    requireDigit: response.requireDigit,
    requireLowercase: response.requireLowercase,
    requireUppercase: response.requireUppercase,
    requireNonAlphanumeric: response.requireNonAlphanumeric,
    requiredUniqueChars: response.requiredUniqueChars,
});

export const mapLoginRequest = (
    credentials: LoginCredentials
): LoginRequest => ({
    email: credentials.email,
    password: credentials.password,
});

export const mapRegisterRequest = (
    credentials: RegisterCredentials
): RegisterRequest => ({
    email: credentials.email,
    password: credentials.password,
});

export const mapRefreshRequest = (
    credentials: RefreshCredentials
): RefreshRequest => ({
    refreshToken: credentials.refreshToken,
});

export const mapAuthTokens = (response: AccessTokenResponse): AuthTokens => ({
    tokenType: response.tokenType ?? "",
    accessToken: response.accessToken,
    expiresIn: response.expiresIn,
    refreshToken: response.refreshToken,
});

export const mapUserInfo = (response: InfoResponse): UserInfo => ({
    email: response.email,
    isEmailConfirmed: response.isEmailConfirmed,
});
