import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { AppSidebar } from "../../../../src/shared/components/layouts/AppSidebar";
import type {
    NavCollection,
    NavUser,
} from "../../../../src/shared/components/layouts/AppSidebar";

describe("AppSidebar", () => {
    const defaultUser: NavUser = {
        initials: "T",
        email: "test@example.com",
    };

    const defaultProps = {
        variant: "permanent" as const,
        draftCount: 0,
        collections: [] as NavCollection[],
        user: defaultUser,
        onAddEntry: vi.fn(),
        onDraftsClick: vi.fn(),
        onCreateCollection: vi.fn(),
    };

    it("does not render the Collections subheader when collections is empty", () => {
        render(<AppSidebar {...defaultProps} />);

        expect(screen.queryByText("Collections")).not.toBeInTheDocument();
    });

    it("renders the empty state sentence when collections is empty", () => {
        render(<AppSidebar {...defaultProps} />);

        expect(screen.getByText("Create")).toBeInTheDocument();
        expect(
            screen.getByText(/collection to get started/)
        ).toBeInTheDocument();
    });

    it("calls onCreateCollection when the link is clicked", async () => {
        const onCreateCollection = vi.fn();
        render(
            <AppSidebar
                {...defaultProps}
                onCreateCollection={onCreateCollection}
            />
        );

        await userEvent.click(screen.getByText("Create"));

        expect(onCreateCollection).toHaveBeenCalledOnce();
    });

    it("does not render the empty state when collections is non-empty", () => {
        const collections: NavCollection[] = [
            {
                id: 1,
                name: "My Collection",
                entryCount: 5,
            },
        ];
        render(<AppSidebar {...defaultProps} collections={collections} />);

        expect(
            screen.queryByText(/collection to get started/)
        ).not.toBeInTheDocument();
    });
});
