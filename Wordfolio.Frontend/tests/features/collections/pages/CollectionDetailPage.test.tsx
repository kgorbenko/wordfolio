import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";

import { CollectionDetailPage } from "../../../../src/features/collections/pages/CollectionDetailPage";

const mockNavigate = vi.fn();

vi.mock("@tanstack/react-router", () => ({
    getRouteApi: () => ({
        useParams: () => ({ collectionId: "col-1" }),
        useSearch: () => ({}),
        useNavigate: () => mockNavigate,
    }),
}));

let mockCollectionQuery = {
    data: { id: "col-1", name: "My Collection", description: null },
    isLoading: false,
    isError: false,
    refetch: vi.fn(),
};

let mockVocabulariesQuery = {
    data: [],
    isLoading: false,
    isError: false,
    refetch: vi.fn(),
};

vi.mock("../../../../src/shared/api/queries/collections", () => ({
    useCollectionQuery: () => mockCollectionQuery,
    useCollectionVocabulariesQuery: () => mockVocabulariesQuery,
}));

vi.mock("../../../../src/shared/api/mutations/collections", () => ({
    useDeleteCollectionMutation: () => ({
        mutate: vi.fn(),
        isPending: false,
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

vi.mock(
    "../../../../src/features/collections/components/CollectionDetailContent",
    () => ({
        CollectionDetailContent: () => <div>Collection Content</div>,
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

describe("CollectionDetailPage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockCollectionQuery = {
            data: { id: "col-1", name: "My Collection", description: null },
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };
        mockVocabulariesQuery = {
            data: [],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };
    });

    it("hides actions when in error state", () => {
        mockCollectionQuery = {
            data: undefined as never,
            isLoading: false,
            isError: true,
            refetch: vi.fn(),
        };

        render(<CollectionDetailPage />, { wrapper: createWrapper() });

        expect(
            screen.queryByRole("button", { name: /edit/i })
        ).not.toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /delete/i })
        ).not.toBeInTheDocument();
    });

    it("shows actions when data loads successfully", () => {
        render(<CollectionDetailPage />, { wrapper: createWrapper() });

        expect(
            screen.getByRole("button", { name: /edit/i })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /delete/i })
        ).toBeInTheDocument();
    });
});
