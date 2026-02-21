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
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
    readonly allowDuplicate?: boolean;
}

export interface UpdateEntryRequest {
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
}

export interface MoveEntryRequest {
    readonly vocabularyId: number;
}

export interface ApiError {
    readonly type?: string;
    readonly title?: string;
    readonly status?: number;
    readonly errors?: Record<string, string[]>;
    readonly error?: string;
    readonly existingEntry?: EntryResponse;
}

export const isDuplicateEntryError = (error: ApiError): boolean =>
    error.status === 409 && error.existingEntry !== undefined;

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
    getEntries: async (
        collectionId: number,
        vocabularyId: number
    ): Promise<EntryResponse[]> => {
        const response = await fetch(
            entriesBasePath(collectionId, vocabularyId),
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
            const error: ApiError = await response.json();
            throw error;
        }

        return response.json();
    },

    createEntry: async (
        collectionId: number,
        vocabularyId: number,
        request: CreateEntryRequest
    ): Promise<EntryResponse> => {
        const response = await fetch(
            entriesBasePath(collectionId, vocabularyId),
            {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify(request),
            }
        );

        if (!response.ok) {
            const errorBody = await response.json();
            const error: ApiError = {
                ...errorBody,
                status: response.status,
            };
            throw error;
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
            const error: ApiError = await response.json();
            throw error;
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
            const error: ApiError = await response.json();
            throw error;
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
            const error: ApiError = await response.json();
            throw error;
        }
    },
};
