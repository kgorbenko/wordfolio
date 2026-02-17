import {
    CollectionSummaryResponse,
    CollectionOverviewResponse,
    CollectionResponse,
    VocabularyResponse,
    VocabularySummaryResponse,
} from "./collectionsApi";
import { Collection, Vocabulary } from "../types";

export const mapCollectionSummary = (
    response: CollectionSummaryResponse
): Collection => ({
    id: response.id,
    name: response.name,
    description: response.description,
    vocabularyCount: response.vocabularies.length,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapCollectionDetail = (
    response: CollectionResponse
): Collection => ({
    id: response.id,
    name: response.name,
    description: response.description,
    vocabularyCount: 0, // TODO: Not available in detail response
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapCollections = (
    responses: CollectionSummaryResponse[]
): Collection[] => responses.map(mapCollectionSummary);

export const mapVocabularySummary = (
    collectionId: number,
    response: VocabularySummaryResponse
): Vocabulary => ({
    id: response.id,
    collectionId,
    name: response.name,
    description: response.description,
    entryCount: response.entryCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapCollectionOverview = (
    response: CollectionOverviewResponse
): Collection => ({
    id: response.id,
    name: response.name,
    description: response.description,
    vocabularyCount: response.vocabularyCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularyResponse = (
    response: VocabularyResponse
): Vocabulary => ({
    id: response.id,
    collectionId: response.collectionId,
    name: response.name,
    description: response.description,
    entryCount: 0, // TODO: Not available in detail response
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularies = (
    responses: VocabularyResponse[]
): Vocabulary[] => responses.map(mapVocabularyResponse);
