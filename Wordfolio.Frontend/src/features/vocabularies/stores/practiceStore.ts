import { create } from "zustand";

import type { Entry } from "../../../shared/api/types/entries";

export type PracticeRating = "easy" | "hard" | "needsWork";

export interface PracticeCard {
    readonly entry: Entry;
    readonly pass: 1 | 2;
}

interface PracticeState {
    readonly queue: PracticeCard[];
    readonly currentIndex: number;
    readonly isFlipped: boolean;
    readonly isComplete: boolean;
}

interface PracticeActions {
    initSession: (entries: Entry[]) => void;
    flip: () => void;
    rate: (rating: PracticeRating) => void;
}

export type PracticeStore = PracticeState & PracticeActions;

export const usePracticeStore = create<PracticeStore>((set) => ({
    queue: [],
    currentIndex: 0,
    isFlipped: false,
    isComplete: false,

    initSession: (entries: Entry[]) => {
        const queue: PracticeCard[] = entries.map((entry) => ({
            entry,
            pass: 1,
        }));
        set({
            queue,
            currentIndex: 0,
            isFlipped: false,
            isComplete: queue.length === 0,
        });
    },

    flip: () => {
        set((state) => ({ isFlipped: !state.isFlipped }));
    },

    rate: (rating: PracticeRating) => {
        set((state) => {
            const currentCard = state.queue[state.currentIndex];
            if (!currentCard) return state;

            let newQueue = [...state.queue];
            if (rating === "needsWork" && currentCard.pass === 1) {
                newQueue = [
                    ...newQueue,
                    { entry: currentCard.entry, pass: 2 as const },
                ];
            }

            const nextIndex = state.currentIndex + 1;
            const isComplete = nextIndex >= newQueue.length;

            return {
                queue: newQueue,
                currentIndex: nextIndex,
                isFlipped: false,
                isComplete,
            };
        });
    },
}));
