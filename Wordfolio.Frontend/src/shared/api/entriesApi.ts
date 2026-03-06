import { useAuthStore } from "../stores/authStore";
import type { ApiError } from "./common";
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

const entriesBasePath = (collectionId: number, vocabularyId: number) =>
    `${API_BASE_URL}/collections/${collectionId}/vocabularies/${vocabularyId}/entries`;

export const createEntry = async (
    collectionId: number,
    vocabularyId: number,
    request: CreateEntryRequest
): Promise<EntryResponse> => {
    const response = await fetch(entriesBasePath(collectionId, vocabularyId), {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        const errorBody = await response.json();
        const error: ApiError = { ...errorBody, status: response.status };
        throw error;
    }

    return response.json();
};
