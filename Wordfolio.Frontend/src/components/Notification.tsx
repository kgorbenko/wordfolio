import { Snackbar, Alert, AlertColor, SnackbarCloseReason } from "@mui/material";

interface INotificationProps {
    readonly isOpen: boolean;
    readonly message: string;
    readonly severity: AlertColor;
    readonly autoHideDuration?: number;
    readonly canBeClosedByUser: boolean;
    readonly onSnackbarClose: (
        event: React.SyntheticEvent | Event,
        reason: SnackbarCloseReason
    ) => void;
    readonly onAlertClose: () => void;
}

export const Notification = ({
    isOpen,
    message,
    severity,
    autoHideDuration,
    canBeClosedByUser,
    onSnackbarClose,
    onAlertClose,
}: INotificationProps) => {
    return (
        <Snackbar
            open={isOpen}
            onClose={onSnackbarClose}
            autoHideDuration={autoHideDuration}
            anchorOrigin={{ vertical: "top", horizontal: "center" }}
        >
            <Alert
                onClose={canBeClosedByUser ? onAlertClose : undefined}
                severity={severity}
            >
                {message}
            </Alert>
        </Snackbar>
    );
};
