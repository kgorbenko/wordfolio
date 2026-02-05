import { useAuthStore } from "../../../stores/authStore";
import { EntryResponse } from "../../../api/entryTypes";

export interface DraftsVocabularyResponse {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
}

export interface DraftsResponse {
    readonly vocabulary: DraftsVocabularyResponse;
    readonly entries: EntryResponse[];
}

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

export const draftsApi = {
    getDrafts: async (): Promise<DraftsResponse | null> => {
        const response = await fetch(`${API_BASE_URL}/drafts`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        if (response.status === 404) {
            return null;
        }

        if (!response.ok) {
            throw new Error("Failed to fetch drafts");
        }

        return response.json();
    },
};
