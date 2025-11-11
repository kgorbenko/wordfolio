import { z } from 'zod';
import { PasswordRequirements } from '../api/authApi';
import { validatePassword } from '../utils/passwordValidation';

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
                .superRefine((password, ctx) => {
                    const result = validatePassword(password, passwordRequirements);
                    if (!result.isValid) {
                        ctx.addIssue({
                            code: z.ZodIssueCode.custom,
                            message: result.message,
                        });
                    }
                }),
            confirmPassword: z.string().min(1, 'Please confirm your password'),
        })
        .refine((data) => data.password === data.confirmPassword, {
            message: 'Passwords do not match',
            path: ['confirmPassword'],
        });
};

export type LoginFormData = z.infer<ReturnType<typeof createLoginSchema>>;
export type RegisterFormData = z.infer<ReturnType<typeof createRegisterSchema>>;
