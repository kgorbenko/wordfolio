import { useAuthStore } from "../../../stores/authStore";
import { EntryResponse } from "../../../api/entryTypes";
import type {
    DefinitionRequest,
    TranslationRequest,
    UpdateEntryRequest,
    MoveEntryRequest,
    ApiError,
} from "../../entries/api/entriesApi";

export interface CreateDraftRequest {
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
    readonly allowDuplicate?: boolean;
}

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
    createDraft: async (
        request: CreateDraftRequest
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
    },

    getDrafts: async (): Promise<DraftsResponse | null> => {
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
            const error: ApiError = await response.json();
            throw error;
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
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    deleteDraftEntry: async (entryId: number): Promise<void> => {
        const response = await fetch(`${API_BASE_URL}/drafts/${entryId}`, {
            method: "DELETE",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
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
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },
};
