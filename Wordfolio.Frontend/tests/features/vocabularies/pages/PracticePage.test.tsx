import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

import { PracticePage } from "../../../../src/features/vocabularies/pages/PracticePage";
import { usePracticeStore } from "../../../../src/features/vocabularies/stores/practiceStore";
import type { Entry } from "../../../../src/shared/api/types/entries";
import {
    DefinitionSource,
    TranslationSource,
} from "../../../../src/shared/api/types/entries";

const mockNavigate = vi.fn();

vi.mock("@tanstack/react-router", () => ({
    useNavigate: () => mockNavigate,
    getRouteApi: () => ({
        useParams: () => ({
            collectionId: "col-1",
            vocabularyId: "voc-1",
        }),
        useNavigate: () => mockNavigate,
    }),
}));

let mockVocabularyData: {
    data: unknown;
    isLoading: boolean;
    isError: boolean;
    refetch: ReturnType<typeof vi.fn>;
} = {
    data: { id: "voc-1", name: "French", collectionName: "Languages" },
    isLoading: false,
    isError: false,
    refetch: vi.fn(),
};

let mockEntriesData: {
    data: Entry[] | undefined;
    isLoading: boolean;
    isError: boolean;
    refetch: ReturnType<typeof vi.fn>;
} = {
    data: [],
    isLoading: false,
    isError: false,
    refetch: vi.fn(),
};

vi.mock("../../../../src/shared/api/queries/vocabularies", () => ({
    useVocabularyDetailQuery: () => mockVocabularyData,
}));

vi.mock("../../../../src/shared/api/queries/entries", () => ({
    useVocabularyEntriesQuery: () => mockEntriesData,
}));

const makeEntry = (id: string, text: string): Entry => ({
    id,
    vocabularyId: "voc-1",
    entryText: text,
    createdAt: new Date(),
    updatedAt: new Date(),
    definitions: [
        {
            id: `${id}-def`,
            definitionText: `${text} definition`,
            source: DefinitionSource.Manual,
            displayOrder: 0,
            examples: [],
        },
    ],
    translations: [
        {
            id: `${id}-trans`,
            translationText: `${text} translation`,
            source: TranslationSource.Manual,
            displayOrder: 0,
            examples: [],
        },
    ],
});

const entry1 = makeEntry("e1", "bonjour");
const entry2 = makeEntry("e2", "merci");

const renderPage = () => {
    const client = new QueryClient({
        defaultOptions: { queries: { retry: false } },
    });
    return render(
        <QueryClientProvider client={client}>
            <PracticePage />
        </QueryClientProvider>
    );
};

describe("PracticePage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockVocabularyData = {
            data: { id: "voc-1", name: "French", collectionName: "Languages" },
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };
        mockEntriesData = {
            data: [],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };
        act(() => {
            usePracticeStore.getState().initSession([]);
        });
    });

    it("renders loading skeleton while data is loading", () => {
        mockEntriesData = {
            data: undefined,
            isLoading: true,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        expect(screen.queryByText("Tap to reveal")).not.toBeInTheDocument();
    });

    it("renders error state when query fails", () => {
        mockEntriesData = {
            data: undefined,
            isLoading: false,
            isError: true,
            refetch: vi.fn(),
        };

        renderPage();

        expect(
            screen.getByText("Failed to Load Vocabulary")
        ).toBeInTheDocument();
    });

    it("renders empty state when vocabulary has no entries", () => {
        mockEntriesData = {
            data: [],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        expect(screen.getByText("No entries to practice")).toBeInTheDocument();
    });

    it("shows first card front text when session starts", () => {
        mockEntriesData = {
            data: [entry1, entry2],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        expect(screen.getByText("bonjour")).toBeInTheDocument();
        expect(screen.getByText("Tap to reveal")).toBeInTheDocument();
    });

    it("shows progress indicator", () => {
        mockEntriesData = {
            data: [entry1, entry2],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        expect(screen.getByText("1 / 2")).toBeInTheDocument();
    });

    it("flips card to show definitions on click", async () => {
        mockEntriesData = {
            data: [entry1],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        await userEvent.click(
            screen.getByRole("button", { name: /card front/i })
        );

        expect(screen.getByText("bonjour definition")).toBeInTheDocument();
        expect(screen.queryByText("Tap to reveal")).not.toBeInTheDocument();
    });

    it("shows rating buttons after flipping", async () => {
        mockEntriesData = {
            data: [entry1],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        await userEvent.click(
            screen.getByRole("button", { name: /card front/i })
        );

        expect(
            screen.getByRole("button", { name: "Easy" })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Hard" })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Needs Work" })
        ).toBeInTheDocument();
    });

    it("advances to next card after rating Easy", async () => {
        mockEntriesData = {
            data: [entry1, entry2],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        await userEvent.click(
            screen.getByRole("button", { name: /card front/i })
        );
        await userEvent.click(screen.getByRole("button", { name: "Easy" }));

        expect(screen.getByText("merci")).toBeInTheDocument();
        expect(screen.getByText("2 / 2")).toBeInTheDocument();
    });

    it("shows completion screen when all cards are rated", async () => {
        mockEntriesData = {
            data: [entry1],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        await userEvent.click(
            screen.getByRole("button", { name: /card front/i })
        );
        await userEvent.click(screen.getByRole("button", { name: "Easy" }));

        expect(screen.getByText("Session Complete!")).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Practice Again" })
        ).toBeInTheDocument();
    });

    it("restarts session when Practice Again is clicked", async () => {
        mockEntriesData = {
            data: [entry1],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        await userEvent.click(
            screen.getByRole("button", { name: /card front/i })
        );
        await userEvent.click(screen.getByRole("button", { name: "Easy" }));
        await userEvent.click(
            screen.getByRole("button", { name: "Practice Again" })
        );

        expect(screen.getByText("bonjour")).toBeInTheDocument();
        expect(screen.getByText("Tap to reveal")).toBeInTheDocument();
    });

    it("requeues a Needs Work card for pass 2", async () => {
        mockEntriesData = {
            data: [entry1, entry2],
            isLoading: false,
            isError: false,
            refetch: vi.fn(),
        };

        renderPage();

        await userEvent.click(
            screen.getByRole("button", { name: /card front/i })
        );
        await userEvent.click(
            screen.getByRole("button", { name: "Needs Work" })
        );

        expect(screen.getByText("merci")).toBeInTheDocument();
        expect(screen.getByText("2 / 3")).toBeInTheDocument();
    });
});
