import { client } from "../../../shared/api/client";
import type { components } from "../../../shared/api/generated/schema";

type RegisterRequest = components["schemas"]["RegisterRequest"];
type LoginRequest = components["schemas"]["LoginRequest"];
type AccessTokenResponse = components["schemas"]["AccessTokenResponse"];
type RefreshRequest = components["schemas"]["RefreshRequest"];
type PasswordRequirementsResponse =
    components["schemas"]["PasswordRequirementsResponse"];
type InfoResponse = components["schemas"]["InfoResponse"];

export const authApi = {
    register: async (request: RegisterRequest): Promise<void> => {
        const { error } = await client.POST("/auth/register", {
            body: request,
        });

        if (error) {
            throw error;
        }
    },

    login: async (request: LoginRequest): Promise<AccessTokenResponse> => {
        const { data, error } = await client.POST("/auth/login", {
            body: request,
        });

        if (error) {
            throw error;
        }

        return data!;
    },

    getPasswordRequirements:
        async (): Promise<PasswordRequirementsResponse> => {
            const { data, error } = await client.GET(
                "/auth/password-requirements"
            );

            if (error) {
                throw error;
            }

            return data!;
        },

    refresh: async (request: RefreshRequest): Promise<AccessTokenResponse> => {
        const { data, error } = await client.POST("/auth/refresh", {
            body: request,
        });

        if (error) {
            throw error;
        }

        return data!;
    },

    getUserInfo: async (): Promise<InfoResponse> => {
        const { data, error } = await client.GET("/auth/manage/info");

        if (error) {
            throw error;
        }

        return data!;
    },
};
