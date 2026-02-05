import { Entry } from "../../types/entry";

export interface DraftsVocabulary {
    readonly id: number;
    readonly name: string;
    readonly description: string | null;
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}

export interface DraftsData {
    readonly vocabulary: DraftsVocabulary;
    readonly entries: Entry[];
}
