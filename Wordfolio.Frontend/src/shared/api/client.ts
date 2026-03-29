import createClient, { type Middleware } from "openapi-fetch";

import type { paths } from "./generated/schema";
import { useAuthStore } from "../stores/authStore";

const authMiddleware: Middleware = {
    async onRequest({ request }) {
        const authTokens = useAuthStore.getState().authTokens;
        if (authTokens) {
            request.headers.set(
                "Authorization",
                `${authTokens.tokenType} ${authTokens.accessToken}`
            );
        }
        return request;
    },
};

export const client = createClient<paths>({ baseUrl: "/api" });
client.use(authMiddleware);

export const getAuthHeaders = (): HeadersInit => {
    const authTokens = useAuthStore.getState().authTokens;
    if (!authTokens) {
        throw new Error("Not authenticated");
    }
    return {
        Accept: "text/event-stream",
        Authorization: `${authTokens.tokenType} ${authTokens.accessToken}`,
    };
};
