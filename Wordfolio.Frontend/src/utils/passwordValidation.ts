import { PasswordRequirements } from '../api/authApi';

export interface PasswordValidationResult {
  readonly isValid: boolean;
  readonly message: string;
}

export function validatePassword(
  password: string,
  requirements: PasswordRequirements
): PasswordValidationResult {
  if (password.length < requirements.requiredLength) {
    return {
      isValid: false,
      message: `Password must be at least ${requirements.requiredLength} characters long`,
    };
  }

  if (requirements.requireDigit && !/\d/.test(password)) {
    return { isValid: false, message: 'Password must contain at least one digit' };
  }

  if (requirements.requireLowercase && !/[a-z]/.test(password)) {
    return { isValid: false, message: 'Password must contain at least one lowercase letter' };
  }

  if (requirements.requireUppercase && !/[A-Z]/.test(password)) {
    return { isValid: false, message: 'Password must contain at least one uppercase letter' };
  }

  if (requirements.requireNonAlphanumeric && !/[^a-zA-Z0-9]/.test(password)) {
    return { isValid: false, message: 'Password must contain at least one non-alphanumeric character' };
  }

  if (requirements.requiredUniqueChars > 0) {
    const uniqueChars = new Set(password).size;
    if (uniqueChars < requirements.requiredUniqueChars) {
      return {
        isValid: false,
        message: `Password must contain at least ${requirements.requiredUniqueChars} unique characters`,
      };
    }
  }

  return { isValid: true, message: '' };
}
