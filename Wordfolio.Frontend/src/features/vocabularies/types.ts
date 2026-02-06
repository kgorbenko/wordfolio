export interface Vocabulary {
    readonly id: number;
    readonly collectionId: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}

export interface VocabularyDetail {
    readonly id: number;
    readonly collectionId: number;
    readonly collectionName: string;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}
