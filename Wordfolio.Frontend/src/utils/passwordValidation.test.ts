import { describe, it, expect } from "vitest";
import { validatePassword } from "./passwordValidation";
import { PasswordRequirements } from "../api/authApi";

describe("validatePassword", () => {
    const defaultRequirements: PasswordRequirements = {
        requiredLength: 8,
        requireDigit: true,
        requireLowercase: true,
        requireUppercase: true,
        requireNonAlphanumeric: false,
        requiredUniqueChars: 0,
    };

    it("should return valid for a password that meets all requirements", () => {
        const result = validatePassword("Password123", defaultRequirements);
        expect(result.isValid).toBe(true);
        expect(result.message).toBe("");
    });

    it("should return invalid if password is shorter than required length", () => {
        const result = validatePassword("Pass1", defaultRequirements);
        expect(result.isValid).toBe(false);
        expect(result.message).toBe(
            "Password must be at least 8 characters long"
        );
    });

    it("should return invalid if password does not contain a digit when required", () => {
        const result = validatePassword("Password", defaultRequirements);
        expect(result.isValid).toBe(false);
        expect(result.message).toBe("Password must contain at least one digit");
    });

    it("should return invalid if password does not contain a lowercase letter when required", () => {
        const result = validatePassword("PASSWORD123", defaultRequirements);
        expect(result.isValid).toBe(false);
        expect(result.message).toBe(
            "Password must contain at least one lowercase letter"
        );
    });

    it("should return invalid if password does not contain an uppercase letter when required", () => {
        const result = validatePassword("password123", defaultRequirements);
        expect(result.isValid).toBe(false);
        expect(result.message).toBe(
            "Password must contain at least one uppercase letter"
        );
    });

    it("should return invalid if password does not contain a non-alphanumeric character when required", () => {
        const requirements: PasswordRequirements = {
            ...defaultRequirements,
            requireNonAlphanumeric: true,
        };
        const result = validatePassword("Password123", requirements);
        expect(result.isValid).toBe(false);
        expect(result.message).toBe(
            "Password must contain at least one non-alphanumeric character"
        );
    });

    it("should return valid if password contains a non-alphanumeric character when required", () => {
        const requirements: PasswordRequirements = {
            ...defaultRequirements,
            requireNonAlphanumeric: true,
        };
        const result = validatePassword("Password123!", requirements);
        expect(result.isValid).toBe(true);
        expect(result.message).toBe("");
    });

    it("should return invalid if password does not have enough unique characters", () => {
        const requirements: PasswordRequirements = {
            ...defaultRequirements,
            requiredUniqueChars: 8,
        };
        const result = validatePassword("AAAAAAa1", requirements);
        expect(result.isValid).toBe(false);
        expect(result.message).toBe(
            "Password must contain at least 8 unique characters"
        );
    });

    it("should return valid if password has enough unique characters", () => {
        const requirements: PasswordRequirements = {
            ...defaultRequirements,
            requiredUniqueChars: 8,
        };
        const result = validatePassword("Password123", requirements);
        expect(result.isValid).toBe(true);
        expect(result.message).toBe("");
    });

    it("should work with minimal requirements", () => {
        const minimalRequirements: PasswordRequirements = {
            requiredLength: 4,
            requireDigit: false,
            requireLowercase: false,
            requireUppercase: false,
            requireNonAlphanumeric: false,
            requiredUniqueChars: 0,
        };
        const result = validatePassword("pass", minimalRequirements);
        expect(result.isValid).toBe(true);
        expect(result.message).toBe("");
    });

    it("should work with strict requirements", () => {
        const strictRequirements: PasswordRequirements = {
            requiredLength: 12,
            requireDigit: true,
            requireLowercase: true,
            requireUppercase: true,
            requireNonAlphanumeric: true,
            requiredUniqueChars: 10,
        };
        const result = validatePassword("Password123!@#", strictRequirements);
        expect(result.isValid).toBe(true);
        expect(result.message).toBe("");
    });

    it("should check requirements in order and return first violation", () => {
        // Length is checked first
        const result = validatePassword("P1", defaultRequirements);
        expect(result.isValid).toBe(false);
        expect(result.message).toBe(
            "Password must be at least 8 characters long"
        );
    });
});
