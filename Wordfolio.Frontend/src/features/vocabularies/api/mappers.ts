import {
    CollectionResponse,
    CreateVocabularyRequest,
    UpdateVocabularyRequest,
    VocabularyResponse,
} from "./vocabulariesApi";
import { Vocabulary, VocabularyCollectionContext } from "../types";
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
