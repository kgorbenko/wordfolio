import type { ReactNode } from "react";
import { Dialog, Drawer, useMediaQuery, useTheme } from "@mui/material";
import type { DialogProps, DrawerProps } from "@mui/material";

import styles from "./ResponsiveDialog.module.scss";

interface ResponsiveDialogProps {
    readonly open: boolean;
    readonly onClose: () => void;
    readonly children: ReactNode;
    readonly maxWidth?: DialogProps["maxWidth"];
    readonly fullWidth?: boolean;
    readonly dialogPaperClassName?: string;
    readonly drawerPaperClassName?: string;
    readonly dialogProps?: Omit<
        DialogProps,
        | "open"
        | "onClose"
        | "children"
        | "PaperProps"
        | "maxWidth"
        | "fullWidth"
    >;
    readonly drawerProps?: Omit<
        DrawerProps,
        "open" | "onClose" | "children" | "anchor" | "PaperProps"
    >;
}

export const ResponsiveDialog = ({
    open,
    onClose,
    children,
    maxWidth,
    fullWidth,
    dialogPaperClassName,
    drawerPaperClassName,
    dialogProps,
    drawerProps,
}: ResponsiveDialogProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    const drawerPaperClass = [styles.drawerPaper, drawerPaperClassName]
        .filter(Boolean)
        .join(" ");

    if (isMobile) {
        return (
            <Drawer
                anchor="bottom"
                open={open}
                onClose={onClose}
                PaperProps={{ className: drawerPaperClass }}
                {...drawerProps}
            >
                {children}
            </Drawer>
        );
    }

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth={maxWidth}
            fullWidth={fullWidth}
            PaperProps={
                dialogPaperClassName
                    ? { className: dialogPaperClassName }
                    : undefined
            }
            {...dialogProps}
        >
            {children}
        </Dialog>
    );
};
