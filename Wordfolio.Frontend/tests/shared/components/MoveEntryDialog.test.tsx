import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { MoveEntryDialog } from "../../../src/shared/components/MoveEntryDialog";
import { draftsValue } from "../../../src/shared/components/VocabularySelector";
import type { CollectionsHierarchy } from "../../../src/shared/api/types/collections";

const useCollectionsHierarchyQueryMock = vi.fn();

vi.mock("../../../src/shared/api/queries/collections", () => ({
    useCollectionsHierarchyQuery: () => useCollectionsHierarchyQueryMock(),
}));

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
            ],
        },
    ],
    defaultVocabulary: {
        id: 99,
        name: "Drafts",
        description: null,
        entryCount: 3,
        createdAt: new Date(),
        updatedAt: new Date(),
    },
    ...overrides,
});

const renderMoveEntryDialog = (currentVocabularyId: number) =>
    render(
        <MoveEntryDialog
            isOpen={true}
            currentVocabularyId={currentVocabularyId}
            onCancel={vi.fn()}
            onConfirm={vi.fn()}
        />
    );

describe("MoveEntryDialog", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("pre-selects drafts when drafts is an available move target", () => {
        useCollectionsHierarchyQueryMock.mockReturnValue({
            data: createHierarchy(),
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        });

        renderMoveEntryDialog(10);

        expect(screen.getByRole("combobox")).toHaveTextContent(
            "Drafts — organize later"
        );
        expect(screen.getByRole("button", { name: "Move" })).toBeEnabled();
    });

    it("does not pre-select a target when drafts is unavailable", () => {
        useCollectionsHierarchyQueryMock.mockReturnValue({
            data: createHierarchy({
                defaultVocabulary: {
                    id: 10,
                    name: "Drafts",
                    description: null,
                    entryCount: 3,
                    createdAt: new Date(),
                    updatedAt: new Date(),
                },
            }),
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        });

        renderMoveEntryDialog(10);

        expect(screen.getByRole("combobox")).not.toHaveTextContent(
            "Drafts — organize later"
        );
        expect(screen.getByRole("button", { name: "Move" })).toBeDisabled();
    });

    it("submits the drafts selection as the default vocabulary target", async () => {
        const onConfirm = vi.fn();

        useCollectionsHierarchyQueryMock.mockReturnValue({
            data: createHierarchy(),
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        });

        render(
            <MoveEntryDialog
                isOpen={true}
                currentVocabularyId={10}
                onCancel={vi.fn()}
                onConfirm={onConfirm}
            />
        );

        screen.getByRole("button", { name: "Move" }).click();

        expect(onConfirm).toHaveBeenCalledWith({
            vocabularyId: undefined,
            isDefault: true,
            collectionId: null,
        });
        expect(useCollectionsHierarchyQueryMock).toHaveBeenCalled();
        expect(draftsValue).toBe(0);
    });
});
