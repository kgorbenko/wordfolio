import { useAuthStore } from "../../../stores/authStore";

export type {
    DefinitionSource,
    TranslationSource,
    ExampleSource,
    ExampleResponse,
    DefinitionResponse,
    TranslationResponse,
    EntryResponse,
} from "../../../api/entryTypes";

import type {
    DefinitionSource,
    ExampleSource,
    EntryResponse,
} from "../../../api/entryTypes";

export interface ExampleRequest {
    readonly exampleText: string;
    readonly source: ExampleSource;
}

export interface DefinitionRequest {
    readonly definitionText: string;
    readonly source: DefinitionSource;
    readonly examples: ExampleRequest[];
}

export interface TranslationRequest {
    readonly translationText: string;
    readonly source: DefinitionSource;
    readonly examples: ExampleRequest[];
}

export interface CreateEntryRequest {
    readonly vocabularyId?: number | null;
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
}

export interface UpdateEntryRequest {
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
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

export const entriesApi = {
    getEntries: async (vocabularyId: number): Promise<EntryResponse[]> => {
        const response = await fetch(
            `${API_BASE_URL}/vocabularies/${vocabularyId}/entries`,
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

    getEntry: async (entryId: number): Promise<EntryResponse> => {
        const response = await fetch(`${API_BASE_URL}/entries/${entryId}`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    createEntry: async (
        request: CreateEntryRequest
    ): Promise<EntryResponse> => {
        const response = await fetch(`${API_BASE_URL}/entries`, {
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

    updateEntry: async (
        entryId: number,
        request: UpdateEntryRequest
    ): Promise<EntryResponse> => {
        const response = await fetch(`${API_BASE_URL}/entries/${entryId}`, {
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

    deleteEntry: async (entryId: number): Promise<void> => {
        const response = await fetch(`${API_BASE_URL}/entries/${entryId}`, {
            method: "DELETE",
            headers: getAuthHeaders(),
        });

        if (!response.ok) {
            const error: ApiError = await response.json();
            throw error;
        }
    },
};
