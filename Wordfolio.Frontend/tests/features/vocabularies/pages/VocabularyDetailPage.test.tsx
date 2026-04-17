import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";

import { VocabularyDetailPage } from "../../../../src/features/vocabularies/pages/VocabularyDetailPage";

const mockNavigate = vi.fn();

vi.mock("@tanstack/react-router", () => ({
    getRouteApi: () => ({
        useParams: () => ({
            collectionId: "col-1",
            vocabularyId: "voc-1",
        }),
        useSearch: () => ({}),
        useNavigate: () => mockNavigate,
    }),
}));

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

let mockEntriesQuery = {
    data: [],
    isLoading: false,
    isError: false,
    refetch: vi.fn(),
};

vi.mock("../../../../src/shared/api/queries/vocabularies", () => ({
    useVocabularyDetailQuery: () => mockVocabularyQuery,
}));

vi.mock("../../../../src/shared/api/queries/entries", () => ({
    useVocabularyEntriesQuery: () => mockEntriesQuery,
}));

vi.mock("../../../../src/shared/api/mutations/vocabularies", () => ({
    useDeleteVocabularyMutation: () => ({
        mutate: vi.fn(),
        isPending: false,
    }),
    useMoveVocabularyMutation: () => ({
        mutate: vi.fn(),
        isPending: false,
    }),
}));

vi.mock(
    "../../../../src/features/vocabularies/hooks/useMoveVocabularyDialog",
    () => ({
        useMoveVocabularyDialog: () => ({
            raiseMoveVocabularyDialogAsync: vi.fn().mockResolvedValue(null),
            dialogElement: null,
        }),
    })
);

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

vi.mock(
    "../../../../src/features/vocabularies/components/VocabularyDetailContent",
    () => ({
        VocabularyDetailContent: () => <div>Vocabulary Content</div>,
    })
);

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

describe("VocabularyDetailPage", () => {
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
        mockEntriesQuery = {
            data: [],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };
    });

    it("hides actions when in error state", () => {
        mockVocabularyQuery = {
            data: undefined as never,
            isLoading: false,
            isError: true,
            refetch: vi.fn(),
        };

        render(<VocabularyDetailPage />, { wrapper: createWrapper() });

        expect(
            screen.queryByRole("button", { name: /edit/i })
        ).not.toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /delete/i })
        ).not.toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /practice/i })
        ).not.toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /move/i })
        ).not.toBeInTheDocument();
    });

    it("shows actions when data loads successfully", () => {
        render(<VocabularyDetailPage />, { wrapper: createWrapper() });

        expect(
            screen.getByRole("button", { name: /edit/i })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /delete/i })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /practice/i })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /move/i })
        ).toBeInTheDocument();
    });

    it("shows updated error title when vocabulary fails to load", () => {
        mockVocabularyQuery = {
            data: undefined as never,
            isLoading: false,
            isError: true,
            refetch: vi.fn(),
        };

        render(<VocabularyDetailPage />, { wrapper: createWrapper() });

        expect(
            screen.getByText("Failed to Load Vocabulary")
        ).toBeInTheDocument();
    });
});
