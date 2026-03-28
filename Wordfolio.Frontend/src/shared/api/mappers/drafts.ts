import type { components } from "../generated/schema";
import type { DraftsVocabulary } from "../types/drafts";

type VocabularyResponse = components["schemas"]["VocabularyResponse"];

export const mapDraftsVocabulary = (
    response: VocabularyResponse
): DraftsVocabulary => ({
    id: Number(response.id),
    name: response.name as string,
    description: response.description as string | null,
    createdAt: new Date(response.createdAt),
    updatedAt: (response.updatedAt as string | null)
        ? new Date(response.updatedAt as string)
        : null,
});
