import { describe, it, expect, beforeEach } from "vitest";
import { act } from "@testing-library/react";

import {
    usePracticeStore,
    type PracticeCard,
} from "../../../../src/features/vocabularies/stores/practiceStore";
import type { Entry } from "../../../../src/shared/api/types/entries";

const makeEntry = (id: string, entryText = id): Entry => ({
    id,
    vocabularyId: "vocab-1",
    entryText,
    createdAt: new Date(),
    updatedAt: new Date(),
    definitions: [],
    translations: [],
});

const entry1 = makeEntry("e1", "hello");
const entry2 = makeEntry("e2", "world");
const entry3 = makeEntry("e3", "test");

describe("usePracticeStore", () => {
    beforeEach(() => {
        act(() => {
            usePracticeStore.getState().initSession([]);
        });
    });

    describe("initSession", () => {
        it("builds a pass-1 queue from entries", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1, entry2]);
            });

            const state = usePracticeStore.getState();
            expect(state.queue).toHaveLength(2);
            expect(state.queue[0]).toEqual<PracticeCard>({
                entry: entry1,
                pass: 1,
            });
            expect(state.queue[1]).toEqual<PracticeCard>({
                entry: entry2,
                pass: 1,
            });
        });

        it("resets currentIndex to 0", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1, entry2]);
                usePracticeStore.getState().rate("easy");
            });

            act(() => {
                usePracticeStore.getState().initSession([entry1]);
            });

            expect(usePracticeStore.getState().currentIndex).toBe(0);
        });

        it("resets isFlipped to false", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
                usePracticeStore.getState().flip();
            });

            act(() => {
                usePracticeStore.getState().initSession([entry1]);
            });

            expect(usePracticeStore.getState().isFlipped).toBe(false);
        });

        it("marks session complete immediately when given empty entries", () => {
            act(() => {
                usePracticeStore.getState().initSession([]);
            });

            expect(usePracticeStore.getState().isComplete).toBe(true);
        });

        it("marks session not complete when entries are provided", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
            });

            expect(usePracticeStore.getState().isComplete).toBe(false);
        });
    });

    describe("flip", () => {
        it("toggles isFlipped from false to true", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
                usePracticeStore.getState().flip();
            });

            expect(usePracticeStore.getState().isFlipped).toBe(true);
        });

        it("toggles isFlipped from true to false", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
                usePracticeStore.getState().flip();
                usePracticeStore.getState().flip();
            });

            expect(usePracticeStore.getState().isFlipped).toBe(false);
        });
    });

    describe("rate", () => {
        it("advances to the next card", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1, entry2]);
                usePracticeStore.getState().rate("easy");
            });

            expect(usePracticeStore.getState().currentIndex).toBe(1);
        });

        it("resets isFlipped after rating", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1, entry2]);
                usePracticeStore.getState().flip();
                usePracticeStore.getState().rate("easy");
            });

            expect(usePracticeStore.getState().isFlipped).toBe(false);
        });

        it("does NOT requeue on easy (pass 1)", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
                usePracticeStore.getState().rate("easy");
            });

            const state = usePracticeStore.getState();
            expect(state.queue).toHaveLength(1);
            expect(state.isComplete).toBe(true);
        });

        it("does NOT requeue on hard (pass 1)", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
                usePracticeStore.getState().rate("hard");
            });

            const state = usePracticeStore.getState();
            expect(state.queue).toHaveLength(1);
            expect(state.isComplete).toBe(true);
        });

        it("requeues on needsWork (pass 1) appended once for pass 2", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
                usePracticeStore.getState().rate("needsWork");
            });

            const state = usePracticeStore.getState();
            expect(state.queue).toHaveLength(2);
            expect(state.queue[1]).toEqual<PracticeCard>({
                entry: entry1,
                pass: 2,
            });
            expect(state.isComplete).toBe(false);
        });

        it("does NOT requeue on needsWork (pass 2)", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1]);
                usePracticeStore.getState().rate("needsWork");
                usePracticeStore.getState().rate("needsWork");
            });

            const state = usePracticeStore.getState();
            expect(state.queue).toHaveLength(2);
            expect(state.isComplete).toBe(true);
        });

        it("marks session complete when last card is rated", () => {
            act(() => {
                usePracticeStore.getState().initSession([entry1, entry2]);
                usePracticeStore.getState().rate("easy");
                usePracticeStore.getState().rate("hard");
            });

            expect(usePracticeStore.getState().isComplete).toBe(true);
        });

        it("handles multiple entries with mixed ratings and requeueing", () => {
            act(() => {
                usePracticeStore
                    .getState()
                    .initSession([entry1, entry2, entry3]);
                usePracticeStore.getState().rate("easy");
                usePracticeStore.getState().rate("needsWork");
                usePracticeStore.getState().rate("hard");
            });

            const state = usePracticeStore.getState();
            expect(state.queue).toHaveLength(4);
            expect(state.queue[3]).toEqual<PracticeCard>({
                entry: entry2,
                pass: 2,
            });
            expect(state.isComplete).toBe(false);

            act(() => {
                usePracticeStore.getState().rate("easy");
            });

            expect(usePracticeStore.getState().isComplete).toBe(true);
        });
    });
});
