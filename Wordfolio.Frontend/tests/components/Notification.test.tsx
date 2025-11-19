import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Notification } from "../../src/components/Notification";

describe("Notification", () => {
    const defaultProps = {
        isOpen: true,
        message: "Test message",
        severity: "info" as const,
        canBeClosedByUser: true,
        onSnackbarClose: vi.fn(),
        onAlertClose: vi.fn(),
    };

    it("should render notification when isOpen is true", () => {
        render(<Notification {...defaultProps} />);
        expect(screen.getByText("Test message")).toBeInTheDocument();
    });

    it("should not render notification when isOpen is false", () => {
        render(<Notification {...defaultProps} isOpen={false} />);
        expect(screen.queryByText("Test message")).not.toBeInTheDocument();
    });

    it("should display correct severity", () => {
        const { container } = render(
            <Notification {...defaultProps} severity="error" />
        );
        const alert = container.querySelector(".MuiAlert-standardError");
        expect(alert).toBeInTheDocument();
    });

    it("should call onAlertClose when close button is clicked", async () => {
        const onAlertClose = vi.fn();
        render(<Notification {...defaultProps} onAlertClose={onAlertClose} />);

        const closeButton = screen.getByRole("button");
        await userEvent.click(closeButton);

        expect(onAlertClose).toHaveBeenCalledTimes(1);
    });

    it("should not show close button when canBeClosedByUser is false", () => {
        render(<Notification {...defaultProps} canBeClosedByUser={false} />);
        expect(screen.queryByRole("button")).not.toBeInTheDocument();
    });

    it("should pass autoHideDuration to Snackbar", () => {
        const { container } = render(
            <Notification {...defaultProps} autoHideDuration={3000} />
        );
        const snackbar = container.querySelector(".MuiSnackbar-root");
        expect(snackbar).toBeInTheDocument();
    });
});
