import { describe, it, expect, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { FormControl, InputLabel } from "@mui/material";
import { GroupedVocabularySelect } from "../../../src/shared/components/GroupedVocabularySelect";
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
            updatedAt: null,
            vocabularies: [
                {
                    id: 10,
                    name: "Idioms",
                    description: null,
                    entryCount: 5,
                    createdAt: new Date(),
                    updatedAt: null,
                },
                {
                    id: 11,
                    name: "Phrasal Verbs",
                    description: null,
                    entryCount: 3,
                    createdAt: new Date(),
                    updatedAt: null,
                },
            ],
        },
        {
            id: 2,
            name: "Spanish Collection",
            description: null,
            createdAt: new Date(),
            updatedAt: null,
            vocabularies: [
                {
                    id: 20,
                    name: "Basic Words",
                    description: null,
                    entryCount: 10,
                    createdAt: new Date(),
                    updatedAt: null,
                },
            ],
        },
    ],
    defaultVocabulary: null,
    ...overrides,
});

const renderGroupedSelect = (
    props?: Partial<React.ComponentProps<typeof GroupedVocabularySelect>>
) => {
    const defaultProps = {
        hierarchy: createHierarchy(),
        value: "" as number | string,
        label: "Target vocabulary",
        onChange: vi.fn(),
        ...props,
    };

    return render(
        <FormControl fullWidth>
            <InputLabel id="test-label">Target vocabulary</InputLabel>
            <GroupedVocabularySelect {...defaultProps} labelId="test-label" />
        </FormControl>
    );
};

describe("GroupedVocabularySelect", () => {
    it("should render collection group headers and vocabulary items", async () => {
        const user = userEvent.setup();
        renderGroupedSelect();

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
        renderGroupedSelect({ onChange });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        await user.click(within(listbox).getByText("English Collection"));

        expect(onChange).not.toHaveBeenCalled();
    });

    it("should render drafts item from default vocabulary", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({
            hierarchy: createHierarchy({
                defaultVocabulary: {
                    id: 99,
                    name: "Default Vocab",
                    description: null,
                    entryCount: 0,
                    createdAt: new Date(),
                    updatedAt: null,
                },
            }),
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(within(listbox).getByText("Drafts")).toBeInTheDocument();
    });

    it("should render custom drafts label when provided", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({
            hierarchy: createHierarchy({
                defaultVocabulary: {
                    id: 99,
                    name: "Default Vocab",
                    description: null,
                    entryCount: 0,
                    createdAt: new Date(),
                    updatedAt: null,
                },
            }),
            draftsLabel: "Drafts — organize later",
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).getByText("Drafts — organize later")
        ).toBeInTheDocument();
    });

    it("should not render drafts item when no default vocabulary and no draftsValue", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({
            hierarchy: createHierarchy({ defaultVocabulary: null }),
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(within(listbox).queryByText("Drafts")).not.toBeInTheDocument();
    });

    it("should always render drafts item when draftsValue is provided", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({
            hierarchy: createHierarchy({ defaultVocabulary: null }),
            draftsLabel: "Drafts — organize later",
            draftsValue: 0,
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).getByText("Drafts — organize later")
        ).toBeInTheDocument();
    });

    it("should exclude specified vocabulary id", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({ excludeVocabularyId: 10 });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(within(listbox).queryByText("Idioms")).not.toBeInTheDocument();
        expect(within(listbox).getByText("Phrasal Verbs")).toBeInTheDocument();
        expect(within(listbox).getByText("Basic Words")).toBeInTheDocument();
    });

    it("should exclude entire collection when all its vocabularies are excluded", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({ excludeVocabularyId: 20 });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(
            within(listbox).queryByText("Spanish Collection")
        ).not.toBeInTheDocument();
        expect(
            within(listbox).queryByText("Basic Words")
        ).not.toBeInTheDocument();
    });

    it("should exclude default vocabulary when it matches excludeVocabularyId", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({
            hierarchy: createHierarchy({
                defaultVocabulary: {
                    id: 99,
                    name: "Default Vocab",
                    description: null,
                    entryCount: 0,
                    createdAt: new Date(),
                    updatedAt: null,
                },
            }),
            excludeVocabularyId: 99,
        });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        expect(within(listbox).queryByText("Drafts")).not.toBeInTheDocument();
    });

    it("should call onChange with correct value when option is selected", async () => {
        const user = userEvent.setup();
        const onChange = vi.fn();
        renderGroupedSelect({ onChange });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        await user.click(within(listbox).getByText("Phrasal Verbs"));

        expect(onChange).toHaveBeenCalledTimes(1);
    });

    it("should render nothing when hierarchy is undefined", async () => {
        const user = userEvent.setup();
        renderGroupedSelect({ hierarchy: undefined });

        await user.click(screen.getByRole("combobox"));

        const listbox = screen.getByRole("listbox");
        const options = within(listbox).queryAllByRole("option");
        expect(options).toHaveLength(0);
    });
});
