import {
    CollectionWithVocabularyCountResponse,
    CollectionResponse,
    CollectionSortBy as ApiCollectionSortBy,
    CreateCollectionRequest,
    GetCollectionVocabulariesQuery,
    SearchUserCollectionsQuery,
    SortDirection as ApiSortDirection,
    UpdateCollectionRequest,
    VocabularySortBy as ApiVocabularySortBy,
    VocabularyWithEntryCountResponse,
} from "./collectionsApi";
import {
    Collection,
    CollectionSearchQuery,
    CollectionSortBy,
    SortDirection,
    Vocabulary,
    CollectionVocabulariesQuery,
    VocabularySortBy,
} from "../types";
import { CollectionFormData } from "../schemas/collectionSchemas";
import { assertNever } from "../../../shared/utils/misc";

export const mapCollectionDetail = (
    response: CollectionResponse
): Collection => ({
    id: response.id,
    name: response.name,
    description: response.description,
    vocabularyCount: 0,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapVocabularyWithEntryCount = (
    collectionId: number,
    response: VocabularyWithEntryCountResponse
): Vocabulary => ({
    id: response.id,
    collectionId,
    name: response.name,
    description: response.description,
    entryCount: response.entryCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapCollectionWithVocabularyCount = (
    response: CollectionWithVocabularyCountResponse
): Collection => ({
    id: response.id,
    name: response.name,
    description: response.description,
    vocabularyCount: response.vocabularyCount,
    createdAt: new Date(response.createdAt),
    updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapToCreateCollectionRequest = (
    data: CollectionFormData
): CreateCollectionRequest => ({
    name: data.name,
    description: data.description,
});

export const mapToUpdateCollectionRequest = (
    data: CollectionFormData
): UpdateCollectionRequest => ({
    name: data.name,
    description: data.description,
});

const mapCollectionSortBy = (sortBy: CollectionSortBy): ApiCollectionSortBy => {
    switch (sortBy) {
        case CollectionSortBy.Name:
            return ApiCollectionSortBy.Name;
        case CollectionSortBy.CreatedAt:
            return ApiCollectionSortBy.CreatedAt;
        case CollectionSortBy.UpdatedAt:
            return ApiCollectionSortBy.UpdatedAt;
        case CollectionSortBy.VocabularyCount:
            return ApiCollectionSortBy.VocabularyCount;
        default:
            return assertNever(sortBy);
    }
};

const mapVocabularySortBy = (sortBy: VocabularySortBy): ApiVocabularySortBy => {
    switch (sortBy) {
        case VocabularySortBy.Name:
            return ApiVocabularySortBy.Name;
        case VocabularySortBy.CreatedAt:
            return ApiVocabularySortBy.CreatedAt;
        case VocabularySortBy.UpdatedAt:
            return ApiVocabularySortBy.UpdatedAt;
        case VocabularySortBy.EntryCount:
            return ApiVocabularySortBy.EntryCount;
        default:
            return assertNever(sortBy);
    }
};

const mapSortDirection = (sortDirection: SortDirection): ApiSortDirection => {
    switch (sortDirection) {
        case SortDirection.Asc:
            return ApiSortDirection.Asc;
        case SortDirection.Desc:
            return ApiSortDirection.Desc;
        default:
            return assertNever(sortDirection);
    }
};

export const mapToSearchUserCollectionsQuery = (
    query: CollectionSearchQuery
): SearchUserCollectionsQuery => ({
    search: query.search,
    sortBy: mapCollectionSortBy(query.sortBy),
    sortDirection: mapSortDirection(query.sortDirection),
});

export const mapToGetCollectionVocabulariesQuery = (
    query: CollectionVocabulariesQuery
): GetCollectionVocabulariesQuery => ({
    collectionId: query.collectionId,
    search: query.search,
    sortBy: mapVocabularySortBy(query.sortBy),
    sortDirection: mapSortDirection(query.sortDirection),
});
