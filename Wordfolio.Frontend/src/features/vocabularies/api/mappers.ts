import {
    CollectionResponse,
    CreateVocabularyRequest,
    EntryResponse,
    UpdateVocabularyRequest,
    VocabularyResponse,
} from "./vocabulariesApi";
import {
    Vocabulary,
    VocabularyCollectionContext,
    VocabularyEntryPreview,
} from "../types";
import { VocabularyFormData } from "../schemas/vocabularySchemas";

export const mapVocabulary = (response: VocabularyResponse): Vocabulary => ({
    id: response.id,
    collectionId: response.collectionId,
    name: response.name,
    description: response.description,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularyCollectionContext = (
    response: CollectionResponse
): VocabularyCollectionContext => ({
    id: response.id,
    name: response.name,
});

const mapVocabularyEntryPreview = (
    response: EntryResponse
): VocabularyEntryPreview => ({
    id: response.id,
    entryText: response.entryText,
    firstDefinition: response.definitions[0]?.definitionText ?? null,
    firstTranslation: response.translations[0]?.translationText ?? null,
    createdAt: new Date(response.createdAt),
});

export const mapVocabularyEntryPreviews = (
    responses: EntryResponse[]
): VocabularyEntryPreview[] => responses.map(mapVocabularyEntryPreview);

export const mapToCreateVocabularyRequest = (
    data: VocabularyFormData
): CreateVocabularyRequest => ({
    name: data.name,
    description: data.description,
});

export const mapToUpdateVocabularyRequest = (
    data: VocabularyFormData
): UpdateVocabularyRequest => ({
    name: data.name,
    description: data.description,
});
