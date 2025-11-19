import { describe, it, expect, vi } from "vitest";
import { render, screen, act } from "@testing-library/react";
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
        const alert = screen.getByTestId("notification-alert");
        expect(alert).toBeInTheDocument();
        expect(screen.getByText("Test message")).toBeInTheDocument();
    });

    it("should not render notification when isOpen is false", () => {
        render(<Notification {...defaultProps} isOpen={false} />);
        expect(
            screen.queryByTestId("notification-alert")
        ).not.toBeInTheDocument();
    });

    it("should display correct severity", () => {
        const { rerender } = render(
            <Notification {...defaultProps} severity="error" />
        );
        const errorAlert = screen.getByTestId("notification-alert");
        const errorClasses = errorAlert.className;

        rerender(<Notification {...defaultProps} severity="success" />);
        const successAlert = screen.getByTestId("notification-alert");
        const successClasses = successAlert.className;

        expect(errorClasses).not.toBe(successClasses);
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

    it("should close notification after autoHideDuration timeout", async () => {
        vi.useFakeTimers();
        const onSnackbarClose = vi.fn();
        render(
            <Notification
                {...defaultProps}
                autoHideDuration={3000}
                onSnackbarClose={onSnackbarClose}
            />
        );

        expect(onSnackbarClose).not.toHaveBeenCalled();

        await act(async () => {
            vi.advanceTimersByTime(3000);
        });

        expect(onSnackbarClose).toHaveBeenCalledTimes(1);

        vi.useRealTimers();
    });

    it("should not close notification by itself without autoHideDuration", async () => {
        vi.useFakeTimers();
        const onSnackbarClose = vi.fn();
        render(
            <Notification {...defaultProps} onSnackbarClose={onSnackbarClose} />
        );

        expect(onSnackbarClose).not.toHaveBeenCalled();

        await act(async () => {
            vi.advanceTimersByTime(10000);
        });

        expect(onSnackbarClose).not.toHaveBeenCalled();

        vi.useRealTimers();
    });
});
