import { useState } from "react";
import { Snackbar, Alert, AlertColor } from "@mui/material";

interface NotificationState {
    open: boolean;
    message: string;
    severity: AlertColor;
}

export const useNotification = () => {
    const [notification, setNotification] = useState<NotificationState>({
        open: false,
        message: "",
        severity: "info",
    });

    const openNotification = (
        message: string,
        severity: AlertColor = "info"
    ) => {
        setNotification({ open: true, message, severity });
    };

    const handleClose = () => {
        setNotification((prev) => ({ ...prev, open: false }));
    };

    const Notification = (
        <Snackbar
            open={notification.open}
            onClose={handleClose}
            anchorOrigin={{ vertical: "top", horizontal: "center" }}
        >
            <Alert onClose={handleClose} severity={notification.severity}>
                {notification.message}
            </Alert>
        </Snackbar>
    );

    return { Notification, openNotification };
};
