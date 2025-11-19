import { expect, afterEach, beforeAll } from "vitest";
import { cleanup } from "@testing-library/react";
import * as matchers from "@testing-library/jest-dom/matchers";

expect.extend(matchers);

// Treat React act warnings as errors
beforeAll(() => {
    const originalError = console.error;
    console.error = (...args: unknown[]) => {
        const message = args[0]?.toString() ?? "";
        if (
            message.includes("was not wrapped in act(...)") ||
            message.includes("inside a test was not wrapped in act")
        ) {
            throw new Error(message);
        }
        originalError(...args);
    };
});

afterEach(() => {
    cleanup();
});
