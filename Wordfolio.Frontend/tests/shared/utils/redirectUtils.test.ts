import { describe, expect, it } from "vitest";

import { getSafeRedirectPath } from "../../../src/shared/utils/redirectUtils";

const fallback = "/";

describe("getSafeRedirectPath", () => {
    it("returns undefined for invalid redirect when no fallback is provided", () => {
        expect(getSafeRedirectPath("https://evil.com/path")).toBeUndefined();
    });

    it("returns undefined for auth-loop redirect when no fallback is provided", () => {
        expect(getSafeRedirectPath("/login")).toBeUndefined();
    });

    it("returns the path when valid and no fallback is provided", () => {
        expect(getSafeRedirectPath("/collections?page=2")).toBe(
            "/collections?page=2"
        );
    });

    it("returns fallback for undefined", () => {
        expect(getSafeRedirectPath(undefined, fallback)).toBe(fallback);
    });

    it("returns fallback for empty string", () => {
        expect(getSafeRedirectPath("", fallback)).toBe(fallback);
    });

    it("returns the path for a safe internal path", () => {
        expect(getSafeRedirectPath("/dashboard", fallback)).toBe("/dashboard");
    });

    it("returns the path for a safe internal path with query string", () => {
        expect(getSafeRedirectPath("/collections?page=2", fallback)).toBe(
            "/collections?page=2"
        );
    });

    it("returns fallback for protocol-relative URL", () => {
        expect(getSafeRedirectPath("//evil.com", fallback)).toBe(fallback);
    });

    it("returns fallback for absolute URL", () => {
        expect(getSafeRedirectPath("https://evil.com/path", fallback)).toBe(
            fallback
        );
    });

    it("returns fallback for /login", () => {
        expect(getSafeRedirectPath("/login", fallback)).toBe(fallback);
    });

    it("returns fallback for /register", () => {
        expect(getSafeRedirectPath("/register", fallback)).toBe(fallback);
    });
});
