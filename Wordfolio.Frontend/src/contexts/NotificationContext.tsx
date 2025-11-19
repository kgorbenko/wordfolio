import { createContext, useContext, useState, useCallback, ReactNode } from "react";
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

interface NotificationContextValue {
    readonly openNotification: (options: OpenNotificationOptions) => void;
    readonly openSuccessNotification: (
        options?: OpenSuccessNotificationOptions
    ) => void;
    readonly openErrorNotification: (
        options?: OpenErrorNotificationOptions
    ) => void;
}

const NotificationContext = createContext<NotificationContextValue | undefined>(
    undefined
);

interface NotificationState {
    readonly message: string;
    readonly severity: AlertColor;
    readonly autoHideDuration?: number;
    readonly canBeClosedByUser: boolean;
}

interface NotificationProviderProps {
    readonly children: ReactNode;
}

export const NotificationProvider = ({ children }: NotificationProviderProps) => {
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

    return (
        <NotificationContext.Provider
            value={{
                openNotification,
                openSuccessNotification,
                openErrorNotification,
            }}
        >
            {children}
            <Notification
                isOpen={notification !== undefined}
                message={notification?.message ?? ""}
                severity={notification?.severity ?? "info"}
                autoHideDuration={notification?.autoHideDuration}
                canBeClosedByUser={notification?.canBeClosedByUser ?? true}
                onSnackbarClose={handleSnackbarClose}
                onAlertClose={handleAlertClose}
            />
        </NotificationContext.Provider>
    );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useNotificationContext = (): NotificationContextValue => {
    const context = useContext(NotificationContext);
    if (context === undefined) {
        throw new Error(
            "useNotificationContext must be used within a NotificationProvider"
        );
    }
    return context;
};
