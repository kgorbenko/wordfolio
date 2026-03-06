import { useAuthStore } from "../stores/authStore";
import type { CreateEntryRequest, EntryResponse } from "./entries";

const API_BASE_URL = "/api";

const getAuthHeaders = (): HeadersInit => {
    const authTokens = useAuthStore.getState().authTokens;
    if (!authTokens) {
        throw new Error("Not authenticated");
    }
    return {
        "Content-Type": "application/json",
        Authorization: `${authTokens.tokenType} ${authTokens.accessToken}`,
    };
};

export const createDraft = async (
    request: CreateEntryRequest
): Promise<EntryResponse> => {
    const response = await fetch(`${API_BASE_URL}/drafts`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        const errorBody = await response.json();
        throw { ...errorBody, status: response.status };
    }

    return response.json();
};
