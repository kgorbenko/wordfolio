import { mapEntry } from "../../../api/entryMappers";
import { DraftsVocabularyResponse } from "./draftsApi";
import { DraftsVocabulary } from "../types";

export { mapEntry };

export const mapDraftsVocabulary = (
    response: DraftsVocabularyResponse
): DraftsVocabulary => ({
    id: response.id,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});
