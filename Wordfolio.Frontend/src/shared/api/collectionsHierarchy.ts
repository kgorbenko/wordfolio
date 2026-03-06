import { useAuthStore } from "../stores/authStore.ts";

export interface VocabularyWithEntryCountResponse {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
    readonly entryCount: number;
}

export interface CollectionWithVocabulariesResponse {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
    readonly vocabularies: VocabularyWithEntryCountResponse[];
}

export interface CollectionsHierarchyResultResponse {
    readonly collections: CollectionWithVocabulariesResponse[];
    readonly defaultVocabulary: VocabularyWithEntryCountResponse | null;
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

export const collectionsHierarchyApi = {
    getCollectionsHierarchy:
        async (): Promise<CollectionsHierarchyResultResponse> => {
            const response = await fetch(
                `${API_BASE_URL}/collections-hierarchy`,
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
