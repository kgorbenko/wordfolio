import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useMediaQuery } from "@mui/material";

import { ResponsiveDialog } from "../../../src/shared/components/ResponsiveDialog";

vi.mock("@mui/material", async (importOriginal) => {
    const actual = await importOriginal<typeof import("@mui/material")>();
    return {
        ...actual,
        useMediaQuery: vi.fn(),
    };
});

describe("ResponsiveDialog", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe("desktop (useMediaQuery returns false)", () => {
        beforeEach(() => {
            vi.mocked(useMediaQuery).mockReturnValue(false);
        });

        it("renders Dialog with children when open", () => {
            render(
                <ResponsiveDialog open={true} onClose={vi.fn()}>
                    <div>dialog content</div>
                </ResponsiveDialog>
            );

            expect(screen.getByRole("dialog")).toBeInTheDocument();
            expect(screen.getByText("dialog content")).toBeInTheDocument();
        });

        it("does not render children when closed", () => {
            render(
                <ResponsiveDialog open={false} onClose={vi.fn()}>
                    <div>hidden content</div>
                </ResponsiveDialog>
            );

            expect(
                screen.queryByText("hidden content")
            ).not.toBeInTheDocument();
        });

        it("calls onClose when Escape is pressed", async () => {
            const onClose = vi.fn();

            render(
                <ResponsiveDialog open={true} onClose={onClose}>
                    <div>content</div>
                </ResponsiveDialog>
            );

            await userEvent.keyboard("{Escape}");

            expect(onClose).toHaveBeenCalledTimes(1);
        });

        it("applies dialogPaperClassName to Dialog paper", () => {
            render(
                <ResponsiveDialog
                    open={true}
                    onClose={vi.fn()}
                    dialogPaperClassName="custom-dialog-class"
                >
                    <div>content</div>
                </ResponsiveDialog>
            );

            const dialog = screen.getByRole("dialog");
            expect(dialog).toHaveClass("custom-dialog-class");
        });
    });

    describe("mobile (useMediaQuery returns true)", () => {
        beforeEach(() => {
            vi.mocked(useMediaQuery).mockReturnValue(true);
        });

        it("renders Drawer with children when open", () => {
            render(
                <ResponsiveDialog open={true} onClose={vi.fn()}>
                    <div>drawer content</div>
                </ResponsiveDialog>
            );

            expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
            expect(screen.getByText("drawer content")).toBeInTheDocument();
        });

        it("does not render children when closed", () => {
            render(
                <ResponsiveDialog open={false} onClose={vi.fn()}>
                    <div>hidden content</div>
                </ResponsiveDialog>
            );

            expect(
                screen.queryByText("hidden content")
            ).not.toBeInTheDocument();
        });
    });
});
