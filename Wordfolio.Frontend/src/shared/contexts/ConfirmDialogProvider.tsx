import { useState, useCallback, useMemo, ReactNode } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Typography,
    Button,
} from "@mui/material";

import { ConfirmDialogContext } from "./ConfirmDialogContext";
import { assertNonNullable } from "../utils/misc.ts";

export interface ConfirmDialogOptions {
    readonly title: string;
    readonly message: string;
    readonly confirmLabel?: string;
    readonly cancelLabel?: string;
    readonly confirmColor?: "primary" | "error";
}

interface ConfirmDialogState {
    readonly title: string;
    readonly message: string;
    readonly confirmLabel?: string;
    readonly cancelLabel?: string;
    readonly confirmColor?: "primary" | "error";
    readonly onConfirm: () => void;
    readonly onReject?: () => void;
}

interface ConfirmDialogProviderProps {
    readonly children: ReactNode;
}

export const ConfirmDialogProvider = ({
    children,
}: ConfirmDialogProviderProps) => {
    const [isOpen, setIsOpen] = useState(false);
    const [dialogState, setDialogState] = useState<
        ConfirmDialogState | undefined
    >(undefined);

    const raiseConfirmDialogAsync = useCallback(
        (options: ConfirmDialogOptions): Promise<boolean> => {
            return new Promise((resolve) => {
                setDialogState({
                    ...options,
                    onConfirm: () => resolve(true),
                    onReject: () => resolve(false),
                });
                setIsOpen(true);
            });
        },
        []
    );

    const handleConfirm = useCallback(() => {
        assertNonNullable(dialogState);

        setIsOpen(false);
        dialogState.onConfirm();
    }, [dialogState]);

    const handleReject = useCallback(() => {
        assertNonNullable(dialogState);

        setIsOpen(false);
        dialogState.onReject?.();
    }, [dialogState]);

    const DialogElement = useMemo(
        () =>
            dialogState !== undefined ? (
                <Dialog open={isOpen} onClose={handleReject}>
                    <DialogTitle>{dialogState.title}</DialogTitle>
                    <DialogContent>
                        <Typography>{dialogState.message}</Typography>
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleReject}>
                            {dialogState.cancelLabel ?? "Cancel"}
                        </Button>
                        <Button
                            variant="contained"
                            color={dialogState.confirmColor ?? "primary"}
                            onClick={handleConfirm}
                        >
                            {dialogState.confirmLabel ?? "Confirm"}
                        </Button>
                    </DialogActions>
                </Dialog>
            ) : undefined,
        [dialogState, handleConfirm, handleReject, isOpen]
    );

    return (
        <ConfirmDialogContext.Provider value={{ raiseConfirmDialogAsync }}>
            {children}
            {DialogElement}
        </ConfirmDialogContext.Provider>
    );
};
