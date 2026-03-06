export interface Vocabulary {
    readonly id: number;
    readonly collectionId: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}

export interface VocabularyCollectionContext {
    readonly id: number;
    readonly name: string;
}

export interface VocabularyEntryPreview {
    readonly id: number;
    readonly entryText: string;
    readonly firstDefinition: string | null;
    readonly firstTranslation: string | null;
    readonly createdAt: Date;
}
