import { describe, it, expect } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
    NotificationProvider,
    useNotificationContext,
} from "./NotificationContext";

const TestComponent = () => {
    const {
        openNotification,
        openSuccessNotification,
        openErrorNotification,
    } = useNotificationContext();

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
            <button onClick={() => openErrorNotification()}>
                Open Error
            </button>
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

describe("NotificationContext", () => {
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
            expect(screen.getByText("Custom notification")).toBeInTheDocument();
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
            expect(
                screen.getByText("Operation completed successfully")
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
            expect(screen.getByText("Custom success")).toBeInTheDocument();
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
            expect(
                screen.getByText("An error occurred. Please try again.")
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
            expect(screen.getByText("Custom error")).toBeInTheDocument();
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
            expect(screen.getByText("Custom notification")).toBeInTheDocument();
        });

        const closeButton = screen.getByRole("button", { name: /close/i });
        await userEvent.click(closeButton);

        await waitFor(() => {
            expect(
                screen.queryByText("Custom notification")
            ).not.toBeInTheDocument();
        });
    });
});
