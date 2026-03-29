import type { components } from "../generated/schema";
import type {
    Vocabulary,
    VocabularyCollectionContext,
    VocabularyDetail,
} from "../types/vocabularies";

type VocabularyResponse = components["schemas"]["VocabularyResponse"];
type VocabularyDetailResponse =
    components["schemas"]["VocabularyDetailResponse"];
type CollectionResponse = components["schemas"]["CollectionResponse"];

export const mapVocabulary = (response: VocabularyResponse): Vocabulary => ({
    id: response.id,
    collectionId: response.collectionId,
    name: response.name,
    description: response.description ?? null,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularyDetail = (
    response: VocabularyDetailResponse
): VocabularyDetail => ({
    id: response.id,
    collectionId: response.collectionId,
    collectionName: response.collectionName,
    name: response.name,
    description: response.description ?? null,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularyCollectionContext = (
    response: CollectionResponse
): VocabularyCollectionContext => ({
    id: response.id,
    name: response.name,
});
