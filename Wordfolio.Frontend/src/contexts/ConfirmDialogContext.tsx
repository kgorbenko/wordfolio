import { createContext, useContext } from "react";

import { ConfirmDialogOptions } from "./ConfirmDialogProvider";

export interface ConfirmDialogContextValue {
    readonly raiseConfirmDialogAsync: (
        options: ConfirmDialogOptions
    ) => Promise<boolean>;
}

export const ConfirmDialogContext = createContext<
    ConfirmDialogContextValue | undefined
>(undefined);

export const useConfirmDialog = (): ConfirmDialogContextValue => {
    const context = useContext(ConfirmDialogContext);
    if (context === undefined) {
        throw new Error(
            "useConfirmDialog must be used within a ConfirmDialogProvider"
        );
    }
    return context;
};
