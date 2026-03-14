import type { EntryResponse } from "./entries";
import { useAuthStore } from "../stores/authStore";

export interface VocabularyDetailResponse {
    readonly id: number;
    readonly collectionId: number;
    readonly collectionName: string;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
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

export const vocabularyApi = {
    getVocabularyDetail: async (
        collectionId: number,
        vocabularyId: number
    ): Promise<VocabularyDetailResponse> => {
        const response = await fetch(
            `${API_BASE_URL}/collections/${collectionId}/vocabularies/${vocabularyId}`,
            {
                method: "GET",
                headers: getAuthHeaders(),
            }
        );

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    getVocabularyEntries: async (
        collectionId: number,
        vocabularyId: number
    ): Promise<EntryResponse[]> => {
        const response = await fetch(
            `${API_BASE_URL}/collections/${collectionId}/vocabularies/${vocabularyId}/entries`,
            {
                method: "GET",
                headers: getAuthHeaders(),
            }
        );

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },
};
