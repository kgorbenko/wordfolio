import type { DraftsVocabulary } from "../types";
import type { VocabularyResponse } from "./draftsApi";

export const mapDraftsVocabulary = (
    response: VocabularyResponse
): DraftsVocabulary => ({
    id: response.id,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});
