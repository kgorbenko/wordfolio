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
