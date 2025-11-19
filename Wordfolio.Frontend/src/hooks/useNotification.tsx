import { useState, useCallback } from "react";
import { AlertColor, SnackbarCloseReason } from "@mui/material";
import { Notification } from "../components/Notification";

interface BaseNotificationOptions {
    readonly autoHideDuration?: number;
    readonly canBeClosedByUser?: boolean;
}

interface OpenNotificationOptions extends BaseNotificationOptions {
    readonly message: string;
    readonly severity: AlertColor;
}

interface OpenSuccessNotificationOptions extends BaseNotificationOptions {
    readonly message?: string;
}

interface OpenErrorNotificationOptions extends BaseNotificationOptions {
    readonly message?: string;
}

interface NotificationState {
    readonly message: string;
    readonly severity: AlertColor;
    readonly autoHideDuration?: number;
    readonly canBeClosedByUser: boolean;
}

export const useNotification = () => {
    const [notification, setNotification] = useState<
        NotificationState | undefined
    >(undefined);

    const openNotification = useCallback((options: OpenNotificationOptions) => {
        setNotification({
            message: options.message,
            severity: options.severity,
            autoHideDuration: options.autoHideDuration,
            canBeClosedByUser: options.canBeClosedByUser ?? true,
        });
    }, []);

    const openSuccessNotification = useCallback(
        (options?: OpenSuccessNotificationOptions) => {
            setNotification({
                message: options?.message ?? "Operation completed successfully",
                severity: "success",
                autoHideDuration: options?.autoHideDuration ?? 6000,
                canBeClosedByUser: options?.canBeClosedByUser ?? true,
            });
        },
        []
    );

    const openErrorNotification = useCallback(
        (options?: OpenErrorNotificationOptions) => {
            setNotification({
                message:
                    options?.message ??
                    "An error occurred. Please try again.",
                severity: "error",
                autoHideDuration: options?.autoHideDuration,
                canBeClosedByUser: options?.canBeClosedByUser ?? true,
            });
        },
        []
    );

    const handleSnackbarClose = useCallback(
        (_event: React.SyntheticEvent | Event, reason: SnackbarCloseReason) => {
            if (reason === "clickaway") {
                return;
            }
            setNotification(undefined);
        },
        []
    );

    const handleAlertClose = useCallback(() => {
        setNotification(undefined);
    }, []);

    const NotificationComponent = (
        <Notification
            isOpen={notification !== undefined}
            message={notification?.message ?? ""}
            severity={notification?.severity ?? "info"}
            autoHideDuration={notification?.autoHideDuration}
            canBeClosedByUser={notification?.canBeClosedByUser ?? true}
            onSnackbarClose={handleSnackbarClose}
            onAlertClose={handleAlertClose}
        />
    );

    return {
        Notification: NotificationComponent,
        openNotification,
        openSuccessNotification,
        openErrorNotification,
    };
};
