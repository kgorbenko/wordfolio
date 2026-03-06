import { useState, useCallback, ReactNode } from "react";
import { AlertColor, SnackbarCloseReason } from "@mui/material";
import { Notification } from "../components/Notification";
import { NotificationContext } from "./NotificationContext";

const defaultAutoHideDuration = 6000;

export interface BaseNotificationOptions {
    readonly autoHideDuration?: number;
    readonly canBeClosedByUser?: boolean;
}

export interface OpenNotificationOptions extends BaseNotificationOptions {
    readonly message: string;
    readonly severity: AlertColor;
}

export interface OpenSuccessNotificationOptions
    extends BaseNotificationOptions {
    readonly message?: string;
}

export interface OpenErrorNotificationOptions extends BaseNotificationOptions {
    readonly message?: string;
}

interface NotificationState {
    readonly message: string;
    readonly severity: AlertColor;
    readonly autoHideDuration?: number;
    readonly canBeClosedByUser: boolean;
}

interface NotificationProviderProps {
    readonly children: ReactNode;
}

export const NotificationProvider = ({
    children,
}: NotificationProviderProps) => {
    const [notification, setNotification] = useState<
        NotificationState | undefined
    >(undefined);

    const openNotification = useCallback((options: OpenNotificationOptions) => {
        setNotification({
            message: options.message,
            severity: options.severity,
            autoHideDuration:
                options.autoHideDuration ?? defaultAutoHideDuration,
            canBeClosedByUser: options.canBeClosedByUser ?? true,
        });
    }, []);

    const openSuccessNotification = useCallback(
        (options?: OpenSuccessNotificationOptions) => {
            setNotification({
                message: options?.message ?? "Operation completed successfully",
                severity: "success",
                autoHideDuration:
                    options?.autoHideDuration ?? defaultAutoHideDuration,
                canBeClosedByUser: options?.canBeClosedByUser ?? true,
            });
        },
        []
    );

    const openErrorNotification = useCallback(
        (options?: OpenErrorNotificationOptions) => {
            setNotification({
                message:
                    options?.message ?? "An error occurred. Please try again.",
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

    return (
        <NotificationContext.Provider
            value={{
                openNotification,
                openSuccessNotification,
                openErrorNotification,
            }}
        >
            {children}
            {notification && (
                <Notification
                    isOpen={true}
                    message={notification.message}
                    severity={notification.severity}
                    autoHideDuration={notification.autoHideDuration}
                    canBeClosedByUser={notification.canBeClosedByUser}
                    onSnackbarClose={handleSnackbarClose}
                    onAlertClose={handleAlertClose}
                />
            )}
        </NotificationContext.Provider>
    );
};
