import { VocabularyResponse } from "./vocabulariesApi";
import { Vocabulary } from "../types";

export const mapVocabulary = (response: VocabularyResponse): Vocabulary => ({
    id: response.id,
    collectionId: response.collectionId,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularies = (
    responses: VocabularyResponse[]
): Vocabulary[] => responses.map(mapVocabulary);
