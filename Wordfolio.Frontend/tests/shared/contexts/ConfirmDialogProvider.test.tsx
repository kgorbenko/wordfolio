import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useMediaQuery } from "@mui/material";

import { ConfirmDialogProvider } from "../../../src/shared/contexts/ConfirmDialogProvider";
import { useConfirmDialog } from "../../../src/shared/contexts/ConfirmDialogContext";

vi.mock("@mui/material", async (importOriginal) => {
    const actual = await importOriginal<typeof import("@mui/material")>();
    return {
        ...actual,
        useMediaQuery: vi.fn(),
    };
});

const TestConsumer = ({
    onResult,
}: {
    readonly onResult: (result: boolean) => void;
}) => {
    const { raiseConfirmDialogAsync } = useConfirmDialog();

    return (
        <button
            onClick={() => {
                void raiseConfirmDialogAsync({
                    title: "Delete item?",
                    message: "This action cannot be undone.",
                    confirmLabel: "Delete",
                    confirmColor: "error",
                }).then(onResult);
            }}
        >
            Open Confirm
        </button>
    );
};

describe("ConfirmDialogProvider", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.mocked(useMediaQuery).mockReturnValue(false);
    });

    it("shows dialog with title and message when opened", async () => {
        render(
            <ConfirmDialogProvider>
                <TestConsumer onResult={vi.fn()} />
            </ConfirmDialogProvider>
        );

        await userEvent.click(
            screen.getByRole("button", { name: "Open Confirm" })
        );

        await waitFor(() => {
            expect(screen.getByText("Delete item?")).toBeInTheDocument();
            expect(
                screen.getByText("This action cannot be undone.")
            ).toBeInTheDocument();
        });
    });

    it("resolves true when the confirm button is clicked", async () => {
        const onResult = vi.fn();

        render(
            <ConfirmDialogProvider>
                <TestConsumer onResult={onResult} />
            </ConfirmDialogProvider>
        );

        await userEvent.click(
            screen.getByRole("button", { name: "Open Confirm" })
        );
        await userEvent.click(screen.getByRole("button", { name: "Delete" }));

        await waitFor(() => {
            expect(onResult).toHaveBeenCalledWith(true);
        });
    });

    it("resolves false when the cancel button is clicked", async () => {
        const onResult = vi.fn();

        render(
            <ConfirmDialogProvider>
                <TestConsumer onResult={onResult} />
            </ConfirmDialogProvider>
        );

        await userEvent.click(
            screen.getByRole("button", { name: "Open Confirm" })
        );
        await userEvent.click(screen.getByRole("button", { name: "Cancel" }));

        await waitFor(() => {
            expect(onResult).toHaveBeenCalledWith(false);
        });
    });

    it("hides the dialog after confirmation", async () => {
        render(
            <ConfirmDialogProvider>
                <TestConsumer onResult={vi.fn()} />
            </ConfirmDialogProvider>
        );

        await userEvent.click(
            screen.getByRole("button", { name: "Open Confirm" })
        );

        await waitFor(() => {
            expect(screen.getByText("Delete item?")).toBeInTheDocument();
        });

        await userEvent.click(screen.getByRole("button", { name: "Delete" }));

        await waitFor(() => {
            expect(screen.queryByText("Delete item?")).not.toBeInTheDocument();
        });
    });
});
