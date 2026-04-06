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
            <Typography
                component="h1"
                sx={{
                    fontFamily: '"DM Sans", sans-serif',
                    fontSize: { xs: "1.5rem", sm: "1.75rem" },
                    fontWeight: 700,
                    color: "#fff",
                    letterSpacing: "-0.03em",
                    lineHeight: 1.1,
                    mb: "6px",
                    textShadow: "0 0 40px rgba(22, 219, 147, 0.16)",
                }}
            >
                {title}
            </Typography>
            <Typography
                component="p"
                variant="overline"
                sx={{
                    fontFamily: '"DM Mono", "Fira Code", monospace',
                    fontSize: "0.5625rem",
                    letterSpacing: "0.18em",
                    textTransform: "uppercase",
                    color: "rgba(22, 219, 147, 0.65)",
                    lineHeight: 1.5,
                    display: "block",
                    mt: 0,
                    mb: 0,
                }}
            >
                {subtitle}
            </Typography>
        </header>

        <div className={styles.content}>{children}</div>

        {footer ? <footer className={styles.footer}>{footer}</footer> : null}
    </section>
);
