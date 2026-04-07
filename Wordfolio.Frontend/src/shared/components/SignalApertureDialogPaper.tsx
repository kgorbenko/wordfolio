import { forwardRef } from "react";

import Paper, { type PaperProps } from "@mui/material/Paper";
import classnames from "classnames";

import styles from "./SignalApertureDialogPaper.module.scss";

export const SignalApertureDialogPaper = forwardRef<HTMLDivElement, PaperProps>(
    ({ className, ...props }, ref) => (
        <Paper
            ref={ref}
            className={classnames(styles.shell, className)}
            {...props}
        />
    )
);

SignalApertureDialogPaper.displayName = "SignalApertureDialogPaper";
