import { useAuthStore } from "../../../shared/stores/authStore";
import type {
    EntryResponse,
    UpdateEntryRequest,
    MoveEntryRequest,
} from "../../../shared/api/entries";

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

const entriesBasePath = (collectionId: number, vocabularyId: number) =>
    `${API_BASE_URL}/collections/${collectionId}/vocabularies/${vocabularyId}/entries`;

export const entriesApi = {
    getEntry: async (
        collectionId: number,
        vocabularyId: number,
        entryId: number
    ): Promise<EntryResponse> => {
        const response = await fetch(
            `${entriesBasePath(collectionId, vocabularyId)}/${entryId}`,
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

    updateEntry: async (
        collectionId: number,
        vocabularyId: number,
        entryId: number,
        request: UpdateEntryRequest
    ): Promise<EntryResponse> => {
        const response = await fetch(
            `${entriesBasePath(collectionId, vocabularyId)}/${entryId}`,
            {
                method: "PUT",
                headers: getAuthHeaders(),
                body: JSON.stringify(request),
            }
        );

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    moveEntry: async (
        collectionId: number,
        vocabularyId: number,
        entryId: number,
        request: MoveEntryRequest
    ): Promise<EntryResponse> => {
        const response = await fetch(
            `${entriesBasePath(collectionId, vocabularyId)}/${entryId}/move`,
            {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify(request),
            }
        );

        if (!response.ok) {
            throw await response.json();
        }

        return response.json();
    },

    deleteEntry: async (
        collectionId: number,
        vocabularyId: number,
        entryId: number
    ): Promise<void> => {
        const response = await fetch(
            `${entriesBasePath(collectionId, vocabularyId)}/${entryId}`,
            {
                method: "DELETE",
                headers: getAuthHeaders(),
            }
        );

        if (!response.ok) {
            throw await response.json();
        }
    },
};
