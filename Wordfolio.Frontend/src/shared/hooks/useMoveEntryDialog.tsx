import { ReactNode, useCallback, useMemo, useState } from "react";

import { assertNonNullable } from "../utils/misc";
import {
    MoveEntryDialog,
    MoveEntrySelectionResult,
} from "../components/MoveEntryDialog";

export interface RaiseMoveEntryDialogOptions {
    readonly currentVocabularyId: number;
}

interface MoveEntryDialogState {
    readonly options: RaiseMoveEntryDialogOptions;
    readonly onConfirm: (value: MoveEntrySelectionResult) => void;
    readonly onReject: () => void;
}

export interface UseMoveEntryDialogResult {
    readonly raiseMoveEntryDialogAsync: (
        options: RaiseMoveEntryDialogOptions
    ) => Promise<MoveEntrySelectionResult | null>;
    readonly dialogElement: ReactNode;
}

export function useMoveEntryDialog(): UseMoveEntryDialogResult {
    const [isOpen, setIsOpen] = useState(false);
    const [dialogState, setDialogState] = useState<
        MoveEntryDialogState | undefined
    >(undefined);

    const raiseMoveEntryDialogAsync = useCallback(
        (
            options: RaiseMoveEntryDialogOptions
        ): Promise<MoveEntrySelectionResult | null> => {
            return new Promise((resolve) => {
                setDialogState({
                    options,
                    onConfirm: (value) => resolve(value),
                    onReject: () => resolve(null),
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

    const handleConfirm = useCallback(
        (moveSelection: MoveEntrySelectionResult) => {
            assertNonNullable(dialogState);

            setIsOpen(false);
            dialogState.onConfirm(moveSelection);
            setDialogState(undefined);
        },
        [dialogState]
    );

    const dialogElement = useMemo(() => {
        if (!dialogState) {
            return undefined;
        }

        return (
            <MoveEntryDialog
                isOpen={isOpen}
                currentVocabularyId={dialogState.options.currentVocabularyId}
                onCancel={handleCancel}
                onConfirm={handleConfirm}
            />
        );
    }, [dialogState, handleCancel, handleConfirm, isOpen]);

    return {
        raiseMoveEntryDialogAsync,
        dialogElement,
    };
}
