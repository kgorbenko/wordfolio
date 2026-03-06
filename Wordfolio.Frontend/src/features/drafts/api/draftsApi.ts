import { useAuthStore } from "../../../shared/stores/authStore";
import type {
    EntryResponse,
    UpdateEntryRequest,
    MoveEntryRequest,
} from "../../../shared/api/entries";

export interface VocabularyResponse {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: string;
    readonly updatedAt: string | null;
}

export interface DraftsVocabularyDataResponse {
    readonly vocabulary: VocabularyResponse;
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
    getDrafts: async (): Promise<DraftsVocabularyDataResponse | null> => {
        const response = await fetch(`${API_BASE_URL}/drafts/all`, {
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

    getDraftEntry: async (entryId: number): Promise<EntryResponse> => {
        const response = await fetch(`${API_BASE_URL}/drafts/${entryId}`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    updateDraftEntry: async (
        entryId: number,
        request: UpdateEntryRequest
    ): Promise<EntryResponse> => {
        const response = await fetch(`${API_BASE_URL}/drafts/${entryId}`, {
            method: "PUT",
            headers: getAuthHeaders(),
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    deleteDraftEntry: async (entryId: number): Promise<void> => {
        const response = await fetch(`${API_BASE_URL}/drafts/${entryId}`, {
            method: "DELETE",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            throw await response.json();
        }
    },

    moveDraftEntry: async (
        entryId: number,
        request: MoveEntryRequest
    ): Promise<EntryResponse> => {
        const response = await fetch(`${API_BASE_URL}/drafts/${entryId}/move`, {
            method: "POST",
            headers: getAuthHeaders(),
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },
};
