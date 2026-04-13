import type { Entry } from "./entries";

export interface DraftsVocabulary {
    readonly id: string;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date;
}

export interface DraftsData {
    readonly vocabulary: DraftsVocabulary;
    readonly entries: Entry[];
}
