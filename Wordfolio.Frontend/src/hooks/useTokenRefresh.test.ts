import { describe, it, expect, vi, beforeEach } from "vitest";

import { authApi } from "../api/authApi";

describe("authApi.refresh", () => {
    const API_BASE_URL = "/api";

    beforeEach(() => {
        vi.clearAllMocks();
        global.fetch = vi.fn();
    });

    it("should call refresh endpoint with correct parameters", async () => {
        const mockResponse = {
            tokenType: "Bearer",
            accessToken: "new-token",
            expiresIn: 3600,
            refreshToken: "new-refresh-token",
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
            ok: true,
            json: async () => mockResponse,
        });

        const request = { refreshToken: "old-refresh-token" };
        const result = await authApi.refresh(request);

        expect(global.fetch).toHaveBeenCalledWith(
            `${API_BASE_URL}/auth/refresh`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(request),
            }
        );

        expect(result).toEqual(mockResponse);
    });

    it("should throw error when refresh fails", async () => {
        const mockError = {
            type: "UnauthorizedError",
            title: "Unauthorized",
            status: 401,
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
            ok: false,
            json: async () => mockError,
        });

        const request = { refreshToken: "invalid-token" };

        await expect(authApi.refresh(request)).rejects.toEqual(mockError);
    });
});
