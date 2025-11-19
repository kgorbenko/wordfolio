import { describe, it, expect } from "vitest";
import { parseApiError } from "../../src/utils/errorHandling";
import { ApiError } from "../../src/api/authApi";

describe("parseApiError", () => {
    it("should return all error messages as an array", () => {
        const error: ApiError = {
            errors: {
                Email: ["Email is required", "Email format is invalid"],
                Password: ["Password is too short"],
            },
        };

        const result = parseApiError(error);

        expect(result).toEqual([
            "Email is required",
            "Email format is invalid",
            "Password is too short",
        ]);
    });

    it("should handle errors from multiple fields", () => {
        const error: ApiError = {
            errors: {
                InvalidEmail: ["Email 'test' is invalid."],
                PasswordTooShort: ["Passwords must be at least 6 characters."],
                PasswordRequiresNonAlphanumeric: [
                    "Passwords must have at least one non alphanumeric character.",
                ],
                PasswordRequiresDigit: [
                    "Passwords must have at least one digit ('0'-'9').",
                ],
                PasswordRequiresUpper: [
                    "Passwords must have at least one uppercase ('A'-'Z').",
                ],
            },
        };

        const result = parseApiError(error);

        expect(result).toEqual([
            "Email 'test' is invalid.",
            "Passwords must be at least 6 characters.",
            "Passwords must have at least one non alphanumeric character.",
            "Passwords must have at least one digit ('0'-'9').",
            "Passwords must have at least one uppercase ('A'-'Z').",
        ]);
    });

    it("should return empty array when no errors exist", () => {
        const error: ApiError = {};

        const result = parseApiError(error);

        expect(result).toEqual([]);
    });

    it("should return empty array when errors object is empty", () => {
        const error: ApiError = {
            errors: {},
        };

        const result = parseApiError(error);

        expect(result).toEqual([]);
    });

    it("should handle multiple messages for the same error key", () => {
        const error: ApiError = {
            errors: {
                Password: [
                    "Password is too short",
                    "Password must contain a digit",
                    "Password must contain uppercase",
                ],
            },
        };

        const result = parseApiError(error);

        expect(result).toEqual([
            "Password is too short",
            "Password must contain a digit",
            "Password must contain uppercase",
        ]);
    });

    it("should handle single error message", () => {
        const error: ApiError = {
            errors: {
                DuplicateUserName: [
                    "User name 'test@example.com' is already taken.",
                ],
            },
        };

        const result = parseApiError(error);

        expect(result).toEqual([
            "User name 'test@example.com' is already taken.",
        ]);
    });
});
