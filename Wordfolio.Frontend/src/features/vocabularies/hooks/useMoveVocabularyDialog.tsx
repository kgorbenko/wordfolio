import { ReactNode, useCallback, useMemo, useState } from "react";

import { assertNonNullable } from "../../../shared/utils/misc";
import {
    MoveVocabularyDialog,
    MoveVocabularySelectionResult,
} from "../components/MoveVocabularyDialog";

export interface RaiseMoveVocabularyDialogOptions {
    readonly currentCollectionId: number;
}

interface MoveVocabularyDialogState {
    readonly options: RaiseMoveVocabularyDialogOptions;
    readonly onConfirm: (value: MoveVocabularySelectionResult) => void;
    readonly onReject: () => void;
}

export interface UseMoveVocabularyDialogResult {
    readonly raiseMoveVocabularyDialogAsync: (
        options: RaiseMoveVocabularyDialogOptions
    ) => Promise<MoveVocabularySelectionResult | null>;
    readonly dialogElement: ReactNode;
}

export function useMoveVocabularyDialog(): UseMoveVocabularyDialogResult {
    const [isOpen, setIsOpen] = useState(false);
    const [dialogState, setDialogState] = useState<
        MoveVocabularyDialogState | undefined
    >(undefined);

    const raiseMoveVocabularyDialogAsync = useCallback(
        (
            options: RaiseMoveVocabularyDialogOptions
        ): Promise<MoveVocabularySelectionResult | null> => {
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
        (moveSelection: MoveVocabularySelectionResult) => {
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
            <MoveVocabularyDialog
                isOpen={isOpen}
                currentCollectionId={dialogState.options.currentCollectionId}
                onCancel={handleCancel}
                onConfirm={handleConfirm}
            />
        );
    }, [dialogState, handleCancel, handleConfirm, isOpen]);

    return {
        raiseMoveVocabularyDialogAsync,
        dialogElement,
    };
}
