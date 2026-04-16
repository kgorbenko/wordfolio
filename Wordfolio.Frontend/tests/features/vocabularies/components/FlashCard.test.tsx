import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { FlashCard } from "../../../../src/features/vocabularies/components/FlashCard";
import type { Entry } from "../../../../src/shared/api/types/entries";
import {
    DefinitionSource,
    TranslationSource,
} from "../../../../src/shared/api/types/entries";

const makeEntry = (overrides: Partial<Entry> = {}): Entry => ({
    id: "e1",
    vocabularyId: "v1",
    entryText: "bonjour",
    createdAt: new Date(),
    updatedAt: new Date(),
    definitions: [],
    translations: [],
    ...overrides,
});

describe("FlashCard", () => {
    it("shows entry text on front side", () => {
        render(
            <FlashCard entry={makeEntry()} isFlipped={false} onFlip={vi.fn()} />
        );

        expect(screen.getByText("bonjour")).toBeInTheDocument();
    });

    it("shows tap hint on front side", () => {
        render(
            <FlashCard entry={makeEntry()} isFlipped={false} onFlip={vi.fn()} />
        );

        expect(screen.getByText("Tap to reveal")).toBeInTheDocument();
    });

    it("shows definitions on back side", () => {
        const entry = makeEntry({
            definitions: [
                {
                    id: "d1",
                    definitionText: "hello in French",
                    source: DefinitionSource.Manual,
                    displayOrder: 0,
                    examples: [],
                },
            ],
        });

        render(<FlashCard entry={entry} isFlipped={true} onFlip={vi.fn()} />);

        expect(screen.getByText("hello in French")).toBeInTheDocument();
    });

    it("shows translations on back side", () => {
        const entry = makeEntry({
            translations: [
                {
                    id: "t1",
                    translationText: "hello",
                    source: TranslationSource.Manual,
                    displayOrder: 0,
                    examples: [],
                },
            ],
        });

        render(<FlashCard entry={entry} isFlipped={true} onFlip={vi.fn()} />);

        expect(screen.getByText("hello")).toBeInTheDocument();
    });

    it("does not show tap hint on back side", () => {
        render(
            <FlashCard entry={makeEntry()} isFlipped={true} onFlip={vi.fn()} />
        );

        expect(screen.queryByText("Tap to reveal")).not.toBeInTheDocument();
    });

    it("calls onFlip when clicked", async () => {
        const onFlip = vi.fn();

        render(
            <FlashCard entry={makeEntry()} isFlipped={false} onFlip={onFlip} />
        );

        await userEvent.click(screen.getByRole("button"));

        expect(onFlip).toHaveBeenCalledTimes(1);
    });

    it("calls onFlip when Enter is pressed", async () => {
        const onFlip = vi.fn();

        render(
            <FlashCard entry={makeEntry()} isFlipped={false} onFlip={onFlip} />
        );

        screen.getByRole("button").focus();
        await userEvent.keyboard("{Enter}");

        expect(onFlip).toHaveBeenCalledTimes(1);
    });

    it("calls onFlip when Space is pressed", async () => {
        const onFlip = vi.fn();

        render(
            <FlashCard entry={makeEntry()} isFlipped={false} onFlip={onFlip} />
        );

        screen.getByRole("button").focus();
        await userEvent.keyboard(" ");

        expect(onFlip).toHaveBeenCalledTimes(1);
    });
});
