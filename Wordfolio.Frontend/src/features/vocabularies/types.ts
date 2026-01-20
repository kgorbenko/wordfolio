export interface Vocabulary {
    readonly id: number;
    readonly collectionId: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}
