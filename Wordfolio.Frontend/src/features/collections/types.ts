export interface Collection {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly vocabularyCount: number;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}

export interface Vocabulary {
    readonly id: number;
    readonly collectionId: number;
    readonly name: string;
    readonly description: string | null;
    readonly entryCount: number;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}

export enum CollectionSortBy {
    Name = 0,
    CreatedAt = 1,
    UpdatedAt = 2,
    VocabularyCount = 3,
}

export enum VocabularySortBy {
    Name = 0,
    CreatedAt = 1,
    UpdatedAt = 2,
    EntryCount = 3,
}

export enum SortDirection {
    Asc = 0,
    Desc = 1,
}

export interface CollectionSearchQuery {
    readonly search?: string;
    readonly sortBy: CollectionSortBy;
    readonly sortDirection: SortDirection;
}

export interface CollectionVocabulariesQuery {
    readonly collectionId: number;
    readonly search?: string;
    readonly sortBy: VocabularySortBy;
    readonly sortDirection: SortDirection;
}
