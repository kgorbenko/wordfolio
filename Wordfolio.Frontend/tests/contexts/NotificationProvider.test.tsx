import { describe, it, expect, vi } from "vitest";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { NotificationProvider } from "../../src/contexts/NotificationProvider";
import { useNotificationContext } from "../../src/contexts/NotificationContext";

const TestComponent = () => {
    const { openNotification, openSuccessNotification, openErrorNotification } =
        useNotificationContext();

    return (
        <div>
            <button
                onClick={() =>
                    openNotification({
                        message: "Custom notification",
                        severity: "info",
                    })
                }
            >
                Open Notification
            </button>
            <button onClick={() => openSuccessNotification()}>
                Open Success
            </button>
            <button
                onClick={() =>
                    openSuccessNotification({ message: "Custom success" })
                }
            >
                Open Custom Success
            </button>
            <button onClick={() => openErrorNotification()}>Open Error</button>
            <button
                onClick={() =>
                    openErrorNotification({ message: "Custom error" })
                }
            >
                Open Custom Error
            </button>
        </div>
    );
};

describe("NotificationProvider", () => {
    it("should throw error when useNotificationContext is used outside provider", () => {
        const consoleError = vi
            .spyOn(console, "error")
            .mockImplementation(() => {});

        expect(() => {
            render(<TestComponent />);
        }).toThrow(
            "useNotificationContext must be used within a NotificationProvider"
        );

        consoleError.mockRestore();
    });

    it("should open custom notification", async () => {
        render(
            <NotificationProvider>
                <TestComponent />
            </NotificationProvider>
        );

        const button = screen.getByText("Open Notification");
        await userEvent.click(button);

        await waitFor(() => {
            const alert = screen.getByTestId("notification-alert");
            expect(
                within(alert).getByText("Custom notification")
            ).toBeInTheDocument();
        });
    });

    it("should open success notification with default message", async () => {
        render(
            <NotificationProvider>
                <TestComponent />
            </NotificationProvider>
        );

        const button = screen.getByText("Open Success");
        await userEvent.click(button);

        await waitFor(() => {
            const alert = screen.getByTestId("notification-alert");
            expect(
                within(alert).getByText("Operation completed successfully")
            ).toBeInTheDocument();
        });
    });

    it("should open success notification with custom message", async () => {
        render(
            <NotificationProvider>
                <TestComponent />
            </NotificationProvider>
        );

        const button = screen.getByText("Open Custom Success");
        await userEvent.click(button);

        await waitFor(() => {
            const alert = screen.getByTestId("notification-alert");
            expect(
                within(alert).getByText("Custom success")
            ).toBeInTheDocument();
        });
    });

    it("should open error notification with default message", async () => {
        render(
            <NotificationProvider>
                <TestComponent />
            </NotificationProvider>
        );

        const button = screen.getByText("Open Error");
        await userEvent.click(button);

        await waitFor(() => {
            const alert = screen.getByTestId("notification-alert");
            expect(
                within(alert).getByText("An error occurred. Please try again.")
            ).toBeInTheDocument();
        });
    });

    it("should open error notification with custom message", async () => {
        render(
            <NotificationProvider>
                <TestComponent />
            </NotificationProvider>
        );

        const button = screen.getByText("Open Custom Error");
        await userEvent.click(button);

        await waitFor(() => {
            const alert = screen.getByTestId("notification-alert");
            expect(within(alert).getByText("Custom error")).toBeInTheDocument();
        });
    });

    it("should close notification when close button is clicked", async () => {
        render(
            <NotificationProvider>
                <TestComponent />
            </NotificationProvider>
        );

        const openButton = screen.getByText("Open Notification");
        await userEvent.click(openButton);

        await waitFor(() => {
            const alert = screen.getByTestId("notification-alert");
            expect(
                within(alert).getByText("Custom notification")
            ).toBeInTheDocument();
        });

        const closeButton = screen.getByRole("button", { name: /close/i });
        await userEvent.click(closeButton);

        await waitFor(() => {
            expect(
                screen.queryByTestId("notification-alert")
            ).not.toBeInTheDocument();
        });
    });
});
