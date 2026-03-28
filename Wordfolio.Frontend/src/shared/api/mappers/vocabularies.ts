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
    id: Number(response.id),
    collectionId: Number(response.collectionId),
    name: response.name as string,
    description: response.description as string | null,
    createdAt: new Date(response.createdAt),
    updatedAt: (response.updatedAt as string | null)
        ? new Date(response.updatedAt as string)
        : null,
});

export const mapVocabularyDetail = (
    response: VocabularyDetailResponse
): VocabularyDetail => ({
    id: Number(response.id),
    collectionId: Number(response.collectionId),
    collectionName: response.collectionName as string,
    name: response.name as string,
    description: response.description as string | null,
    createdAt: new Date(response.createdAt),
    updatedAt: (response.updatedAt as string | null)
        ? new Date(response.updatedAt as string)
        : null,
});

export const mapVocabularyCollectionContext = (
    response: CollectionResponse
): VocabularyCollectionContext => ({
    id: Number(response.id),
    name: response.name as string,
});
