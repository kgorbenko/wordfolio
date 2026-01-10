import { create } from "zustand";

interface UiState {
    readonly isWordEntryOpen: boolean;
    readonly selectedVocabularyId: number | null;
    openWordEntry: (vocabularyId?: number) => void;
    closeWordEntry: () => void;
}

export const useUiStore = create<UiState>()((set) => ({
    isWordEntryOpen: false,
    selectedVocabularyId: null,
    openWordEntry: (vocabularyId?: number) =>
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
