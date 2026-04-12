import { create } from "zustand";

interface UiState {
    readonly isWordEntryOpen: boolean;
    readonly selectedVocabularyId: string | null;
    openWordEntry: (vocabularyId?: string) => void;
    closeWordEntry: () => void;
}

export const useUiStore = create<UiState>()((set) => ({
    isWordEntryOpen: false,
    selectedVocabularyId: null,
    openWordEntry: (vocabularyId?: string) =>
        set({
            isWordEntryOpen: true,
            selectedVocabularyId: vocabularyId ?? null,
        }),
    closeWordEntry: () =>
        set({
            isWordEntryOpen: false,
            selectedVocabularyId: null,
        }),
}));
