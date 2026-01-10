import { useAuthStore } from "../stores/authStore";

export type DefinitionSource = "Api" | "Manual";
export type ExampleSource = "Api" | "Custom";

export interface ExampleResponse {
    readonly id: number;
    readonly exampleText: string;
    readonly source: ExampleSource;
}

export interface DefinitionResponse {
    readonly id: number;
    readonly definitionText: string;
    readonly source: DefinitionSource;
    readonly displayOrder: number;
    readonly examples: ExampleResponse[];
}

export interface TranslationResponse {
    readonly id: number;
    readonly translationText: string;
    readonly source: DefinitionSource;
    readonly displayOrder: number;
    readonly examples: ExampleResponse[];
}

export interface EntryResponse {
    readonly id: number;
    readonly vocabularyId: number;
    readonly entryText: string;
    readonly createdAt: string;
    readonly updatedAt: string | null;
    readonly definitions: DefinitionResponse[];
    readonly translations: TranslationResponse[];
}

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
    readonly vocabularyId: number;
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
