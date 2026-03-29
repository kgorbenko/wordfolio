import type { components } from "../generated/schema";
import type { DraftsVocabulary } from "../types/drafts";

type VocabularyResponse = components["schemas"]["VocabularyResponse"];

export const mapDraftsVocabulary = (
    response: VocabularyResponse
): DraftsVocabulary => ({
    id: response.id,
    name: response.name,
    description: response.description ?? null,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});
