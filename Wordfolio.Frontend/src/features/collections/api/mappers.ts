import {
    CollectionWithVocabularyCountResponse,
    CollectionResponse,
    CreateCollectionRequest,
    UpdateCollectionRequest,
    VocabularyWithEntryCountResponse,
} from "./collectionsApi";
import {
    Collection,
    CollectionWithVocabularyCount,
    VocabularyWithEntryCount,
} from "../types";
import { CollectionFormData } from "../schemas/collectionSchemas";

export const mapCollectionDetail = (
    response: CollectionResponse
): Collection => ({
    id: response.id,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularyWithEntryCount = (
    collectionId: number,
    response: VocabularyWithEntryCountResponse
): VocabularyWithEntryCount => ({
    id: response.id,
    collectionId,
    name: response.name,
    description: response.description,
    entryCount: response.entryCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapCollectionWithVocabularyCount = (
    response: CollectionWithVocabularyCountResponse
): CollectionWithVocabularyCount => ({
    id: response.id,
    name: response.name,
    description: response.description,
    vocabularyCount: response.vocabularyCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapToCreateCollectionRequest = (
    data: CollectionFormData
): CreateCollectionRequest => ({
    name: data.name,
    description: data.description,
});

export const mapToUpdateCollectionRequest = (
    data: CollectionFormData
): UpdateCollectionRequest => ({
    name: data.name,
    description: data.description,
});
