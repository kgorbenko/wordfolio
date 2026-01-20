import { useAuthStore } from "../../../stores/authStore";

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

export interface ApiError {
    readonly type?: string;
    readonly title?: string;
    readonly status?: number;
    readonly errors?: Record<string, string[]>;
}

export interface VocabularyResponse {
    readonly id: number;
    readonly collectionId: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
}

export interface VocabularySummaryResponse {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
    readonly entryCount: number;
}

export interface CollectionSummaryResponse {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
    readonly vocabularies: VocabularySummaryResponse[];
}

export interface CollectionsHierarchyResponse {
    readonly collections: CollectionSummaryResponse[];
    readonly defaultVocabulary: VocabularySummaryResponse | null;
}

export interface CollectionResponse {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
}

export interface CreateCollectionRequest {
    readonly name: string;
    readonly description?: string | null;
}

export interface UpdateCollectionRequest {
    readonly name: string;
    readonly description?: string | null;
}

export const collectionsApi = {
    getAll: async (): Promise<CollectionSummaryResponse[]> => {
        const response = await fetch(`${API_BASE_URL}/collections-hierarchy`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        const data: CollectionsHierarchyResponse = await response.json();
        return data.collections;
    },

    getById: async (id: number): Promise<CollectionResponse> => {
        const response = await fetch(`${API_BASE_URL}/collections/${id}`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    create: async (
        request: CreateCollectionRequest
    ): Promise<CollectionResponse> => {
        const response = await fetch(`${API_BASE_URL}/collections`, {
            method: "POST",
            headers: getAuthHeaders(),
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    update: async (
        id: number,
        request: UpdateCollectionRequest
    ): Promise<CollectionResponse> => {
        const response = await fetch(`${API_BASE_URL}/collections/${id}`, {
            method: "PUT",
            headers: getAuthHeaders(),
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    delete: async (id: number): Promise<void> => {
        const response = await fetch(`${API_BASE_URL}/collections/${id}`, {
            method: "DELETE",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }
    },

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
};
