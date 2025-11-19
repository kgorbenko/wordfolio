import { describe, it, expect } from "vitest";
import { parseApiError } from "../../src/utils/errorHandling";
import { ApiError } from "../../src/api/authApi";

describe("parseApiError", () => {
    it("should parse field-specific errors", () => {
        const error: ApiError = {
            errors: {
                Email: ["Email is required", "Email format is invalid"],
                Password: ["Password is too short"],
            },
        };

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({
            email: ["Email is required", "Email format is invalid"],
            password: ["Password is too short"],
        });
        expect(result.generalErrors).toEqual([]);
    });

    it("should handle case-insensitive field matching", () => {
        const error: ApiError = {
            errors: {
                EMAIL: ["Email is invalid"],
                PaSsWoRd: ["Password is weak"],
            },
        };

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({
            email: ["Email is invalid"],
            password: ["Password is weak"],
        });
        expect(result.generalErrors).toEqual([]);
    });

    it("should separate unmapped errors as general errors", () => {
        const error: ApiError = {
            errors: {
                DuplicateUserName: [
                    "User name 'test@example.com' is already taken.",
                ],
                Email: ["Email is required"],
            },
        };

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({
            email: ["Email is required"],
        });
        expect(result.generalErrors).toEqual([
            "User name 'test@example.com' is already taken.",
        ]);
    });

    it("should handle multiple unmapped errors", () => {
        const error: ApiError = {
            errors: {
                DuplicateUserName: ["Username already exists"],
                PasswordTooShort: ["Password must be at least 8 characters"],
                Email: ["Email is invalid"],
            },
        };

        const result = parseApiError(error, ["email"]);

        expect(result.fieldErrors).toEqual({
            email: ["Email is invalid"],
        });
        expect(result.generalErrors).toEqual([
            "Username already exists",
            "Password must be at least 8 characters",
        ]);
    });

    it("should return empty arrays when no errors exist", () => {
        const error: ApiError = {};

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({});
        expect(result.generalErrors).toEqual([]);
    });

    it("should handle error with empty errors object", () => {
        const error: ApiError = {
            errors: {},
        };

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({});
        expect(result.generalErrors).toEqual([]);
    });

    it("should handle all errors as general when no valid fields match", () => {
        const error: ApiError = {
            errors: {
                DuplicateUserName: ["Username exists"],
                PasswordTooShort: ["Password too short"],
            },
        };

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({});
        expect(result.generalErrors).toEqual([
            "Username exists",
            "Password too short",
        ]);
    });

    it("should handle multiple messages for the same field", () => {
        const error: ApiError = {
            errors: {
                Password: [
                    "Password is too short",
                    "Password must contain a digit",
                    "Password must contain uppercase",
                ],
            },
        };

        const result = parseApiError(error, ["password"]);

        expect(result.fieldErrors).toEqual({
            password: [
                "Password is too short",
                "Password must contain a digit",
                "Password must contain uppercase",
            ],
        });
        expect(result.generalErrors).toEqual([]);
    });

    it("should handle mixed case with unmapped errors", () => {
        const error: ApiError = {
            errors: {
                email: ["Email is invalid"],
                PASSWORD: ["Password is weak"],
                UnknownError: ["Something went wrong"],
            },
        };

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({
            email: ["Email is invalid"],
            password: ["Password is weak"],
        });
        expect(result.generalErrors).toEqual(["Something went wrong"]);
    });

    it("should handle fields that are not in the valid field names list", () => {
        const error: ApiError = {
            errors: {
                Email: ["Email is required"],
                Username: ["Username is taken"],
                Password: ["Password is weak"],
            },
        };

        const result = parseApiError(error, ["email", "password"]);

        expect(result.fieldErrors).toEqual({
            email: ["Email is required"],
            password: ["Password is weak"],
        });
        expect(result.generalErrors).toEqual(["Username is taken"]);
    });
});
