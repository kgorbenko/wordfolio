import { describe, it, expect } from 'vitest';
import { validateEmail, validatePassword, formatUsername } from './validation';

describe('validateEmail', () => {
    it('should return true for valid email addresses', () => {
        expect(validateEmail('user@example.com')).toBe(true);
        expect(validateEmail('test.user@domain.co.uk')).toBe(true);
        expect(validateEmail('name+tag@company.org')).toBe(true);
    });

    it('should return false for invalid email addresses', () => {
        expect(validateEmail('invalid')).toBe(false);
        expect(validateEmail('invalid@')).toBe(false);
        expect(validateEmail('@domain.com')).toBe(false);
        expect(validateEmail('user@domain')).toBe(false);
        expect(validateEmail('')).toBe(false);
    });
});

describe('validatePassword', () => {
    it('should validate a strong password', () => {
        const result = validatePassword('StrongPass123');
        expect(result.valid).toBe(true);
        expect(result.errors).toHaveLength(0);
    });

    it('should reject password shorter than 8 characters', () => {
        const result = validatePassword('Pass1');
        expect(result.valid).toBe(false);
        expect(result.errors).toContain('Password must be at least 8 characters long');
    });

    it('should reject password without uppercase letter', () => {
        const result = validatePassword('password123');
        expect(result.valid).toBe(false);
        expect(result.errors).toContain('Password must contain at least one uppercase letter');
    });

    it('should reject password without lowercase letter', () => {
        const result = validatePassword('PASSWORD123');
        expect(result.valid).toBe(false);
        expect(result.errors).toContain('Password must contain at least one lowercase letter');
    });

    it('should reject password without number', () => {
        const result = validatePassword('PasswordOnly');
        expect(result.valid).toBe(false);
        expect(result.errors).toContain('Password must contain at least one number');
    });

    it('should return multiple errors for weak password', () => {
        const result = validatePassword('weak');
        expect(result.valid).toBe(false);
        expect(result.errors.length).toBeGreaterThan(1);
    });
});

describe('formatUsername', () => {
    it('should trim and lowercase username', () => {
        expect(formatUsername('  JohnDoe  ')).toBe('johndoe');
        expect(formatUsername('ADMIN')).toBe('admin');
        expect(formatUsername('User Name')).toBe('user name');
    });

    it('should handle already formatted usernames', () => {
        expect(formatUsername('johndoe')).toBe('johndoe');
        expect(formatUsername('user123')).toBe('user123');
    });

    it('should handle empty string', () => {
        expect(formatUsername('')).toBe('');
        expect(formatUsername('   ')).toBe('');
    });
});
