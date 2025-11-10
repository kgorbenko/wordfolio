import { describe, it, expect } from 'vitest';
import { validateEmail, formatUsername } from './validation';

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
