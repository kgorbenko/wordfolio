import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";

import { EntryDetailPage } from "../../../../src/features/entries/pages/EntryDetailPage";

const mockNavigate = vi.fn();
const mockInvalidateQueries = vi.fn();

vi.mock("@tanstack/react-router", () => ({
    useNavigate: () => mockNavigate,
    getRouteApi: () => ({
        useParams: () => ({
            collectionId: "col-1",
            vocabularyId: "voc-1",
            entryId: "entry-1",
        }),
        useNavigate: () => mockNavigate,
    }),
}));

vi.mock("@tanstack/react-query", async (importOriginal) => {
    const actual =
        await importOriginal<typeof import("@tanstack/react-query")>();
    return {
        ...actual,
        useQueryClient: () => ({
            invalidateQueries: mockInvalidateQueries,
        }),
    };
});

let mockVocabularyQuery = {
    data: {
        id: "voc-1",
        name: "French",
        collectionName: "Languages",
        description: null,
    },
    isLoading: false,
    isError: false,
    refetch: vi.fn(),
};

let mockEntryQuery = {
    data: {
        id: "entry-1",
        entryText: "bonjour",
        vocabularyId: "voc-1",
        createdAt: new Date(),
        updatedAt: new Date(),
        definitions: [],
        translations: [],
    },
    isLoading: false,
    isError: false,
    refetch: vi.fn(),
};

vi.mock("../../../../src/shared/api/queries/vocabularies", () => ({
    useVocabularyDetailQuery: () => mockVocabularyQuery,
}));

vi.mock("../../../../src/shared/api/queries/entries", () => ({
    useEntryQuery: () => mockEntryQuery,
}));

vi.mock("../../../../src/shared/api/mutations/entries", () => ({
    useDeleteEntryMutation: () => ({
        mutate: vi.fn(),
        isPending: false,
    }),
    useMoveEntryMutation: () => ({
        mutate: vi.fn(),
        isPending: false,
    }),
}));

vi.mock("../../../../src/shared/hooks/useMoveEntryDialog", () => ({
    useMoveEntryDialog: () => ({
        raiseMoveEntryDialogAsync: vi.fn().mockResolvedValue(null),
        dialogElement: null,
    }),
}));

vi.mock("../../../../src/shared/contexts/ConfirmDialogContext", () => ({
    useConfirmDialog: () => ({
        raiseConfirmDialogAsync: vi.fn().mockResolvedValue(false),
    }),
}));

vi.mock("../../../../src/shared/contexts/NotificationContext", () => ({
    useNotificationContext: () => ({
        openErrorNotification: vi.fn(),
    }),
}));

vi.mock("../../../../src/shared/components/entries/EntryDetailContent", () => ({
    EntryDetailContent: () => <div>Entry Content</div>,
}));

const createWrapper = () => {
    const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
    });
    return ({ children }: { children: ReactNode }) => (
        <QueryClientProvider client={queryClient}>
            {children}
        </QueryClientProvider>
    );
};

describe("EntryDetailPage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockVocabularyQuery = {
            data: {
                id: "voc-1",
                name: "French",
                collectionName: "Languages",
                description: null,
            },
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };
        mockEntryQuery = {
            data: {
                id: "entry-1",
                entryText: "bonjour",
                vocabularyId: "voc-1",
                createdAt: new Date(),
                updatedAt: new Date(),
                definitions: [],
                translations: [],
            },
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };
    });

    it("hides actions when in error state", () => {
        mockEntryQuery = {
            data: undefined as never,
            isLoading: false,
            isError: true,
            refetch: vi.fn(),
        };

        render(<EntryDetailPage />, { wrapper: createWrapper() });

        expect(
            screen.queryByRole("button", { name: /edit/i })
        ).not.toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /delete/i })
        ).not.toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /move/i })
        ).not.toBeInTheDocument();
    });

    it("shows actions when data loads successfully", () => {
        render(<EntryDetailPage />, { wrapper: createWrapper() });

        expect(
            screen.getByRole("button", { name: /edit/i })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /delete/i })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /move/i })
        ).toBeInTheDocument();
    });
});
