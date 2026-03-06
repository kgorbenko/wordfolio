import type {
    CollectionsHierarchyResultResponse,
    CollectionWithVocabulariesResponse,
    VocabularyWithEntryCountResponse,
} from "./collectionsHierarchy";
import type {
    CollectionsHierarchy,
    CollectionWithVocabularies,
    VocabularyWithEntryCount,
} from "../types/collectionsHierarchy";

const mapVocabularyWithEntryCount = (
    response: VocabularyWithEntryCountResponse
): VocabularyWithEntryCount => ({
    id: response.id,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
    entryCount: response.entryCount,
});

const mapCollectionWithVocabularies = (
    response: CollectionWithVocabulariesResponse
): CollectionWithVocabularies => ({
    id: response.id,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
    vocabularies: response.vocabularies.map(mapVocabularyWithEntryCount),
});

export const mapCollectionsHierarchy = (
    response: CollectionsHierarchyResultResponse
): CollectionsHierarchy => ({
    collections: response.collections.map(mapCollectionWithVocabularies),
    defaultVocabulary: response.defaultVocabulary
        ? mapVocabularyWithEntryCount(response.defaultVocabulary)
        : null,
});
