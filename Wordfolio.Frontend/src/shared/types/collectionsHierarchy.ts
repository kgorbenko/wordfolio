export interface VocabularyWithEntryCount {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
    readonly entryCount: number;
}

export interface CollectionWithVocabularies {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
    readonly vocabularies: VocabularyWithEntryCount[];
}

export interface CollectionsHierarchy {
    readonly collections: CollectionWithVocabularies[];
    readonly defaultVocabulary: VocabularyWithEntryCount | null;
}
