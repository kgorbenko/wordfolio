import { useAuthStore } from "../../../stores/authStore";

export interface VocabularyResponse {
    readonly id: number;
    readonly collectionId: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
}

export interface CreateVocabularyRequest {
    readonly name: string;
    readonly description?: string | null;
}

export interface UpdateVocabularyRequest {
    readonly name: string;
    readonly description?: string | null;
}

export interface ApiError {
    readonly type?: string;
    readonly title?: string;
    readonly status?: number;
    readonly errors?: Record<string, string[]>;
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

export const vocabulariesApi = {
    getVocabularies: async (
        collectionId: number
    ): Promise<VocabularyResponse[]> => {
        const response = await fetch(
            `${API_BASE_URL}/collections/${collectionId}/vocabularies`,
            {
                method: "GET",
                headers: getAuthHeaders(),
            }
        );

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    createVocabulary: async (
        collectionId: number,
        request: CreateVocabularyRequest
    ): Promise<VocabularyResponse> => {
        const response = await fetch(
            `${API_BASE_URL}/collections/${collectionId}/vocabularies`,
            {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify(request),
            }
        );

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    getVocabulary: async (
        collectionId: number,
        vocabularyId: number
    ): Promise<VocabularyResponse> => {
        const response = await fetch(
            `${API_BASE_URL}/collections/${collectionId}/vocabularies/${vocabularyId}`,
            {
                method: "GET",
                headers: getAuthHeaders(),
            }
        );

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    updateVocabulary: async (
        collectionId: number,
        vocabularyId: number,
        request: UpdateVocabularyRequest
    ): Promise<VocabularyResponse> => {
        const response = await fetch(
            `${API_BASE_URL}/collections/${collectionId}/vocabularies/${vocabularyId}`,
            {
                method: "PUT",
                headers: getAuthHeaders(),
                body: JSON.stringify(request),
            }
        );

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    deleteVocabulary: async (
        collectionId: number,
        vocabularyId: number
    ): Promise<void> => {
        const response = await fetch(
            `${API_BASE_URL}/collections/${collectionId}/vocabularies/${vocabularyId}`,
            {
                method: "DELETE",
                headers: getAuthHeaders(),
            }
        );

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }
    },
};
