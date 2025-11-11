import { z } from 'zod';
import { PasswordRequirements } from '../api/authApi';

export const createLoginSchema = () => {
  return z.object({
    email: z.string().min(1, 'Email is required').email('Invalid email address'),
    password: z.string().min(1, 'Password is required'),
  });
};

export const createRegisterSchema = (passwordRequirements: PasswordRequirements) => {
  return z
    .object({
      email: z.string().min(1, 'Email is required').email('Invalid email address'),
      password: z
        .string()
        .min(1, 'Password is required')
        .min(passwordRequirements.requiredLength, `Password must be at least ${passwordRequirements.requiredLength} characters long`)
        .refine(
          (password) => !passwordRequirements.requireDigit || /\d/.test(password),
          'Password must contain at least one digit'
        )
        .refine(
          (password) => !passwordRequirements.requireLowercase || /[a-z]/.test(password),
          'Password must contain at least one lowercase letter'
        )
        .refine(
          (password) => !passwordRequirements.requireUppercase || /[A-Z]/.test(password),
          'Password must contain at least one uppercase letter'
        )
        .refine(
          (password) => !passwordRequirements.requireNonAlphanumeric || /[^a-zA-Z0-9]/.test(password),
          'Password must contain at least one non-alphanumeric character'
        )
        .refine(
          (password) => {
            if (passwordRequirements.requiredUniqueChars === 0) return true;
            const uniqueChars = new Set(password).size;
            return uniqueChars >= passwordRequirements.requiredUniqueChars;
          },
          `Password must contain at least ${passwordRequirements.requiredUniqueChars} unique characters`
        ),
      confirmPassword: z.string().min(1, 'Please confirm your password'),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: 'Passwords do not match',
      path: ['confirmPassword'],
    });
};

export type LoginFormData = z.infer<ReturnType<typeof createLoginSchema>>;
export type RegisterFormData = z.infer<ReturnType<typeof createRegisterSchema>>;
