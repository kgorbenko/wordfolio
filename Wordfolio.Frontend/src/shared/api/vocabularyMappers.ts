import type { VocabularyDetailResponse } from "./vocabularies";
import type { VocabularyDetail } from "../types/vocabularies";

export const mapVocabularyDetail = (
    response: VocabularyDetailResponse
): VocabularyDetail => ({
    id: response.id,
    collectionId: response.collectionId,
    collectionName: response.collectionName,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});
