import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ReactNode } from "react";

import type { BreadcrumbItem } from "../../../../src/shared/components/layouts/BreadcrumbNav";
import { BreadcrumbNav } from "../../../../src/shared/components/layouts/BreadcrumbNav";

const {
    mockHistoryBack,
    mockCanGoBack,
    mockNavigate,
    mockUseRouter,
    mockUseNavigate,
} = vi.hoisted(() => ({
    mockHistoryBack: vi.fn(),
    mockCanGoBack: vi.fn(),
    mockNavigate: vi.fn(),
    mockUseRouter: vi.fn(),
    mockUseNavigate: vi.fn(),
}));

vi.mock("@tanstack/react-router", () => ({
    useRouter: mockUseRouter,
    useNavigate: mockUseNavigate,
    Link: ({
        children,
        className,
    }: {
        to: string;
        params?: Record<string, string | number>;
        children: ReactNode;
        className?: string;
    }) => (
        <a href="#" className={className}>
            {children}
        </a>
    ),
}));

const threeItems: BreadcrumbItem[] = [
    { label: "Collections", to: "/collections" },
    {
        label: "My Vocabulary",
        to: "/vocabularies/1",
        params: { vocabularyId: "1" },
    },
    { label: "Entry Name" },
];

describe("BreadcrumbNav", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        mockUseNavigate.mockReturnValue(mockNavigate);
        mockCanGoBack.mockReturnValue(false);
        mockUseRouter.mockReturnValue({
            history: { back: mockHistoryBack, canGoBack: mockCanGoBack },
        });
    });

    describe("non-truncated mode", () => {
        it("renders all items", () => {
            render(<BreadcrumbNav items={threeItems} />);

            expect(screen.getByText("Collections")).toBeInTheDocument();
            expect(screen.getByText("My Vocabulary")).toBeInTheDocument();
            expect(screen.getByText("Entry Name")).toBeInTheDocument();
        });

        it("renders separators between items", () => {
            render(<BreadcrumbNav items={threeItems} />);

            expect(screen.getAllByText("/")).toHaveLength(2);
        });

        it("renders non-active items as links", () => {
            render(<BreadcrumbNav items={threeItems} />);

            expect(
                screen.getByRole("link", { name: "Collections" })
            ).toBeInTheDocument();
            expect(
                screen.getByRole("link", { name: "My Vocabulary" })
            ).toBeInTheDocument();
        });

        it("renders the last item as plain text rather than a link", () => {
            render(<BreadcrumbNav items={threeItems} />);

            expect(
                screen.queryByRole("link", { name: "Entry Name" })
            ).not.toBeInTheDocument();
        });
    });

    describe("truncated mode", () => {
        it("renders only the icon back button and current page label for multiple items", () => {
            render(<BreadcrumbNav items={threeItems} truncate />);

            expect(screen.getByRole("button")).toBeInTheDocument();
            expect(screen.getByText("Entry Name")).toBeInTheDocument();
            expect(screen.queryByText("Collections")).not.toBeInTheDocument();
            expect(screen.queryByText("My Vocabulary")).not.toBeInTheDocument();
        });

        it("renders no back button for a single item", () => {
            render(
                <BreadcrumbNav items={[{ label: "Collections" }]} truncate />
            );

            expect(screen.queryByRole("button")).not.toBeInTheDocument();
            expect(screen.getByText("Collections")).toBeInTheDocument();
        });

        it("renders no separator between back button and current page label", () => {
            render(<BreadcrumbNav items={threeItems} truncate />);

            expect(screen.queryByText("/")).not.toBeInTheDocument();
        });

        it("calls history.back when the router has prior history", async () => {
            mockCanGoBack.mockReturnValue(true);

            render(<BreadcrumbNav items={threeItems} truncate />);
            await userEvent.click(screen.getByRole("button"));

            expect(mockHistoryBack).toHaveBeenCalledOnce();
            expect(mockNavigate).not.toHaveBeenCalled();
        });

        it("navigates to the parent route when there is no prior history", async () => {
            render(<BreadcrumbNav items={threeItems} truncate />);
            await userEvent.click(screen.getByRole("button"));

            expect(mockNavigate).toHaveBeenCalledWith({
                to: "/vocabularies/1",
                params: { vocabularyId: "1" },
            });
            expect(mockHistoryBack).not.toHaveBeenCalled();
        });
    });
});
