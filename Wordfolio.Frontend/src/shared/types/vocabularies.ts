export interface VocabularyDetail {
    readonly id: number;
    readonly collectionId: number;
    readonly collectionName: string;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}
