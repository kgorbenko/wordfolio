import type { components } from "../generated/schema";
import type {
    Collection,
    CollectionWithVocabularies,
    CollectionWithVocabularyCount,
    CollectionsHierarchy,
    VocabularyWithEntryCount,
} from "../types/collections";

type CollectionResponse = components["schemas"]["CollectionResponse"];
type CollectionWithVocabulariesResponse =
    components["schemas"]["CollectionWithVocabulariesResponse"];
type CollectionWithVocabularyCountResponse =
    components["schemas"]["CollectionWithVocabularyCountResponse"];
type CollectionsHierarchyResultResponse =
    components["schemas"]["CollectionsHierarchyResultResponse"];
type VocabularyWithEntryCountResponse =
    components["schemas"]["VocabularyWithEntryCountResponse"];

export const mapCollectionDetail = (
    response: CollectionResponse
): Collection => ({
    id: response.id,
    name: response.name,
    description: response.description ?? null,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapCollectionWithVocabularyCount = (
    response: CollectionWithVocabularyCountResponse
): CollectionWithVocabularyCount => ({
    id: response.id,
    name: response.name,
    description: response.description ?? null,
    vocabularyCount: response.vocabularyCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularyWithEntryCount = (
    response: VocabularyWithEntryCountResponse
): VocabularyWithEntryCount => ({
    id: response.id,
    name: response.name,
    description: response.description ?? null,
    entryCount: response.entryCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

const mapCollectionWithVocabularies = (
    response: CollectionWithVocabulariesResponse
): CollectionWithVocabularies => ({
    id: response.id,
    name: response.name,
    description: response.description ?? null,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
    vocabularies: response.vocabularies.map((v) =>
        mapVocabularyWithEntryCount(v)
    ),
});

export const mapCollectionsHierarchy = (
    response: CollectionsHierarchyResultResponse
): CollectionsHierarchy => ({
    collections: response.collections.map(mapCollectionWithVocabularies),
    defaultVocabulary: response.defaultVocabulary
        ? mapVocabularyWithEntryCount(response.defaultVocabulary)
        : null,
});
