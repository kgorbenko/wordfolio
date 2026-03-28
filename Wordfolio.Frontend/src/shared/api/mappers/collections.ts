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
    id: Number(response.id),
    name: response.name as string,
    description: response.description as string | null,
    createdAt: new Date(response.createdAt),
    updatedAt: (response.updatedAt as string | null)
        ? new Date(response.updatedAt as string)
        : null,
});

export const mapCollectionWithVocabularyCount = (
    response: CollectionWithVocabularyCountResponse
): CollectionWithVocabularyCount => ({
    id: Number(response.id),
    name: response.name as string,
    description: response.description as string | null,
    vocabularyCount: Number(response.vocabularyCount),
    createdAt: new Date(response.createdAt),
    updatedAt: (response.updatedAt as string | null)
        ? new Date(response.updatedAt as string)
        : null,
});

export const mapVocabularyWithEntryCount = (
    response: VocabularyWithEntryCountResponse,
    collectionId?: number
): VocabularyWithEntryCount => ({
    id: Number(response.id),
    collectionId,
    name: response.name as string,
    description: response.description as string | null,
    entryCount: Number(response.entryCount),
    createdAt: new Date(response.createdAt),
    updatedAt: (response.updatedAt as string | null)
        ? new Date(response.updatedAt as string)
        : null,
});

const mapCollectionWithVocabularies = (
    response: CollectionWithVocabulariesResponse
): CollectionWithVocabularies => ({
    id: Number(response.id),
    name: response.name as string,
    description: response.description as string | null,
    createdAt: new Date(response.createdAt),
    updatedAt: (response.updatedAt as string | null)
        ? new Date(response.updatedAt as string)
        : null,
    vocabularies: (response.vocabularies ?? []).map((v) =>
        mapVocabularyWithEntryCount(v)
    ),
});

export const mapCollectionsHierarchy = (
    response: CollectionsHierarchyResultResponse
): CollectionsHierarchy => ({
    collections: (response.collections ?? []).map(
        mapCollectionWithVocabularies
    ),
    defaultVocabulary: (response.defaultVocabulary as
        | CollectionsHierarchyResultResponse["defaultVocabulary"]
        | null)
        ? mapVocabularyWithEntryCount(response.defaultVocabulary)
        : null,
});
