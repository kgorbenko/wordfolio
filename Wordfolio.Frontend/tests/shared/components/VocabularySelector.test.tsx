import { describe, it, expect, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
    draftsValue,
    VocabularySelector,
} from "../../../src/shared/components/VocabularySelector";
import type { CollectionsHierarchy } from "../../../src/shared/api/types/collections";

const createHierarchy = (
    overrides?: Partial<CollectionsHierarchy>
): CollectionsHierarchy => ({
    collections: [
        {
            id: 1,
            name: "English Collection",
            description: null,
            createdAt: new Date(),
            updatedAt: new Date(),
            vocabularies: [
                {
                    id: 10,
                    name: "Idioms",
                    description: null,
                    entryCount: 5,
                    createdAt: new Date(),
                    updatedAt: new Date(),
                },
                {
                    id: 11,
                    name: "Phrasal Verbs",
                    description: null,
                    entryCount: 3,
                    createdAt: new Date(),
                    updatedAt: new Date(),
                },
            ],
        },
        {
            id: 2,
            name: "Spanish Collection",
            description: null,
            createdAt: new Date(),
            updatedAt: new Date(),
            vocabularies: [
                {
                    id: 20,
                    name: "Basic Words",
                    description: null,
                    entryCount: 10,
                    createdAt: new Date(),
                    updatedAt: new Date(),
                },
            ],
        },
    ],
    defaultVocabulary: null,
    ...overrides,
});

const renderVocabularySelector = (
    props?: Partial<React.ComponentProps<typeof VocabularySelector>>
) => {
    const defaultProps = {
        hierarchy: createHierarchy(),
        value: undefined as number | undefined,
        label: "Target vocabulary",
        onChange: vi.fn() as (value: number) => void,
        ...props,
    };

    return render(<VocabularySelector {...defaultProps} />);
};

describe("VocabularySelector", () => {
    it("should render collection group headers and vocabulary items", async () => {
        const user = userEvent.setup();
        renderVocabularySelector();

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).getByText("English Collection")
        ).toBeInTheDocument();
        expect(
            within(listbox).getByText("Spanish Collection")
        ).toBeInTheDocument();
        expect(within(listbox).getByText("Idioms")).toBeInTheDocument();
        expect(within(listbox).getByText("Phrasal Verbs")).toBeInTheDocument();
        expect(within(listbox).getByText("Basic Words")).toBeInTheDocument();
    });

    it("should not trigger onChange when clicking a group header", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        renderVocabularySelector({ onChange });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        await user.click(within(listbox).getByText("English Collection"));

        expect(onChange).not.toHaveBeenCalled();
    });

    it("should render drafts item even when no default vocabulary", async () => {
        const user = userEvent.setup();
        renderVocabularySelector({
            hierarchy: createHierarchy({ defaultVocabulary: null }),
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).getByText("Drafts — organize later")
        ).toBeInTheDocument();
    });

    it("should render drafts item when default vocabulary is set", async () => {
        const user = userEvent.setup();
        renderVocabularySelector({
            hierarchy: createHierarchy({
                defaultVocabulary: {
                    id: 99,
                    name: "Default Vocab",
                    description: null,
                    entryCount: 0,
                    createdAt: new Date(),
                    updatedAt: new Date(),
                },
            }),
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).getByText("Drafts — organize later")
        ).toBeInTheDocument();
    });

    it("should exclude specified vocabulary id", async () => {
        const user = userEvent.setup();
        renderVocabularySelector({ excludeVocabularyId: 10 });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(within(listbox).queryByText("Idioms")).not.toBeInTheDocument();
        expect(within(listbox).getByText("Phrasal Verbs")).toBeInTheDocument();
        expect(within(listbox).getByText("Basic Words")).toBeInTheDocument();
    });

    it("should exclude entire collection when all its vocabularies are excluded", async () => {
        const user = userEvent.setup();
        renderVocabularySelector({ excludeVocabularyId: 20 });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).queryByText("Spanish Collection")
        ).not.toBeInTheDocument();
        expect(
            within(listbox).queryByText("Basic Words")
        ).not.toBeInTheDocument();
    });

    it("should hide drafts item when excludeVocabularyId matches default vocabulary", async () => {
        const user = userEvent.setup();
        renderVocabularySelector({
            hierarchy: createHierarchy({
                defaultVocabulary: {
                    id: 99,
                    name: "Default Vocab",
                    description: null,
                    entryCount: 0,
                    createdAt: new Date(),
                    updatedAt: new Date(),
                },
            }),
            excludeVocabularyId: 99,
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).queryByText("Drafts — organize later")
        ).not.toBeInTheDocument();
    });

    it("should call onChange with the vocabulary id when a vocabulary option is selected", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        renderVocabularySelector({ onChange });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        await user.click(within(listbox).getByText("Phrasal Verbs"));

        expect(onChange).toHaveBeenCalledWith(11);
    });

    it("should call onChange with draftsValue when the Drafts option is selected", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        renderVocabularySelector({
            value: 11,
            onChange,
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        await user.click(within(listbox).getByText("Drafts — organize later"));

        expect(onChange).toHaveBeenCalledWith(draftsValue);
    });

    it("should render nothing when hierarchy is undefined", async () => {
        const user = userEvent.setup();
        renderVocabularySelector({ hierarchy: undefined });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        const options = within(listbox).queryAllByRole("option");
        expect(options).toHaveLength(0);
    });
});
