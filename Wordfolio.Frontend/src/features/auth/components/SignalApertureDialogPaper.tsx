import type { ReactNode } from "react";

import { Typography } from "@mui/material";

import { ApertureRing } from "../../../shared/components/aperture/ApertureRing";

import styles from "./SignalApertureDialogPaper.module.scss";

type SignalApertureDialogPaperProps = {
    readonly title: string;
    readonly subtitle: string;
    readonly children: ReactNode;
    readonly footer?: ReactNode;
};

export const SignalApertureDialogPaper = ({
    title,
    subtitle,
    children,
    footer,
}: SignalApertureDialogPaperProps) => (
    <section className={styles.paper}>
        <header className={styles.header}>
            <div className={styles.apertureIcon} aria-hidden="true">
                <ApertureRing className={styles.apertureRing} />
            </div>
            <Typography component="h1" className={styles.title}>
                {title}
            </Typography>
            <p className={styles.subtitle}>{subtitle}</p>
        </header>

        <div className={styles.content}>{children}</div>

        {footer ? <footer className={styles.footer}>{footer}</footer> : null}
    </section>
);
