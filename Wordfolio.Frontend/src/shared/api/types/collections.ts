export interface Collection {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date;
}

export interface CollectionWithVocabularyCount {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly vocabularyCount: number;
    readonly createdAt: Date;
    readonly updatedAt: Date;
}

export interface VocabularyWithEntryCount {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly entryCount: number;
    readonly createdAt: Date;
    readonly updatedAt: Date;
}

export interface CollectionWithVocabularies {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date;
    readonly vocabularies: VocabularyWithEntryCount[];
}

export interface CollectionsHierarchy {
    readonly collections: CollectionWithVocabularies[];
    readonly defaultVocabulary: VocabularyWithEntryCount | null;
}
