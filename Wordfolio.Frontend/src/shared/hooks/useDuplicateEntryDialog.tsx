import { ReactNode, useCallback, useMemo, useState } from "react";

import type { Entry } from "../types/entries";
import { assertNonNullable } from "../utils/misc";
import { DuplicateEntryDialog } from "../components/entries/DuplicateEntryDialog";

interface DuplicateEntryDialogState {
    readonly existingEntry: Entry;
    readonly onConfirm: () => void;
    readonly onReject: () => void;
}

export interface UseDuplicateEntryDialogResult {
    readonly raiseDuplicateEntryDialogAsync: (
        existingEntry: Entry
    ) => Promise<boolean>;
    readonly dialogElement: ReactNode;
}

export function useDuplicateEntryDialog(): UseDuplicateEntryDialogResult {
    const [isOpen, setIsOpen] = useState(false);
    const [dialogState, setDialogState] = useState<
        DuplicateEntryDialogState | undefined
    >(undefined);

    const raiseDuplicateEntryDialogAsync = useCallback(
        (existingEntry: Entry): Promise<boolean> => {
            return new Promise((resolve) => {
                setDialogState({
                    existingEntry,
                    onConfirm: () => resolve(true),
                    onReject: () => resolve(false),
                });
                setIsOpen(true);
            });
        },
        []
    );

    const handleCancel = useCallback(() => {
        assertNonNullable(dialogState);

        setIsOpen(false);
        dialogState.onReject();
        setDialogState(undefined);
    }, [dialogState]);

    const handleConfirm = useCallback(() => {
        assertNonNullable(dialogState);

        setIsOpen(false);
        dialogState.onConfirm();
        setDialogState(undefined);
    }, [dialogState]);

    const dialogElement = useMemo(() => {
        if (!dialogState) {
            return undefined;
        }

        return (
            <DuplicateEntryDialog
                isOpen={isOpen}
                existingEntry={dialogState.existingEntry}
                onCancel={handleCancel}
                onConfirm={handleConfirm}
            />
        );
    }, [dialogState, handleCancel, handleConfirm, isOpen]);

    return {
        raiseDuplicateEntryDialogAsync,
        dialogElement,
    };
}
