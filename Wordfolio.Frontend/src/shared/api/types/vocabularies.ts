export interface Vocabulary {
    readonly id: string;
    readonly collectionId: string;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date;
}

export interface VocabularyDetail {
    readonly id: string;
    readonly collectionId: string;
    readonly collectionName: string;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date;
}

export interface VocabularyCollectionContext {
    readonly id: string;
    readonly name: string;
}
