import { useAuthStore } from "../stores/authStore";

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

export interface CreateVocabularyRequest {
    readonly name: string;
    readonly description?: string | null;
}

export interface UpdateVocabularyRequest {
    readonly name: string;
    readonly description?: string | null;
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
    getCollectionsHierarchy:
        async (): Promise<CollectionsHierarchyResponse> => {
            const response = await fetch(
                `${API_BASE_URL}/collections-hierarchy`,
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

    getCollections: async (): Promise<CollectionResponse[]> => {
        const response = await fetch(`${API_BASE_URL}/collections`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    createCollection: async (
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

    getCollection: async (id: number): Promise<CollectionResponse> => {
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

    updateCollection: async (
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

    deleteCollection: async (id: number): Promise<void> => {
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

    getAllVocabularies: async (): Promise<
        Array<VocabularyResponse & { collectionName: string }>
    > => {
        const collections = await vocabulariesApi.getCollections();
        const allVocabularies: Array<
            VocabularyResponse & { collectionName: string }
        > = [];

        for (const collection of collections) {
            const vocabularies = await vocabulariesApi.getVocabularies(
                collection.id
            );
            for (const vocab of vocabularies) {
                allVocabularies.push({
                    ...vocab,
                    collectionName: collection.name,
                });
            }
        }

        return allVocabularies;
    },

    getOrCreateDefaultVocabulary: async (): Promise<VocabularyResponse> => {
        const collections = await vocabulariesApi.getCollections();
        let unsortedCollection = collections.find((c) => c.name === "Unsorted");

        if (!unsortedCollection) {
            unsortedCollection = await vocabulariesApi.createCollection({
                name: "Unsorted",
                description: "Default collection for quick word entries",
            });
        }

        const vocabularies = await vocabulariesApi.getVocabularies(
            unsortedCollection.id
        );
        let defaultVocabulary = vocabularies.find((v) => v.name === "My Words");

        if (!defaultVocabulary) {
            defaultVocabulary = await vocabulariesApi.createVocabulary(
                unsortedCollection.id,
                {
                    name: "My Words",
                    description: "Default vocabulary for quick word entries",
                }
            );
        }

        return defaultVocabulary;
    },
};
