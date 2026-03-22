export interface PasswordRequirements {
    readonly requiredLength: number;
    readonly requireDigit: boolean;
    readonly requireLowercase: boolean;
    readonly requireUppercase: boolean;
    readonly requireNonAlphanumeric: boolean;
    readonly requiredUniqueChars: number;
}

export interface LoginCredentials {
    readonly email: string;
    readonly password: string;
}

export interface RegisterCredentials {
    readonly email: string;
    readonly password: string;
}

export interface RefreshCredentials {
    readonly refreshToken: string;
}

export interface AuthTokens {
    readonly tokenType: string;
    readonly accessToken: string;
    readonly expiresIn: number;
    readonly refreshToken: string;
}

export interface UserInfo {
    readonly email: string;
    readonly isEmailConfirmed: boolean;
}
