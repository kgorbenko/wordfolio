import type { CSSProperties, ReactNode } from "react";

import { ApertureRing } from "../../../shared/components/aperture/ApertureRing";

import styles from "./SignalApertureAuthBackground.module.scss";

type GhostWord = {
    readonly text: string;
    readonly x: number;
    readonly y: number;
    readonly size: number;
    readonly blur: number;
    readonly opacity: number;
    readonly delay: number;
    readonly magenta?: boolean;
};

const ghostWords: GhostWord[] = [
    {
        text: "threshold",
        x: 6,
        y: 12,
        size: 0.78,
        blur: 2.2,
        opacity: 0.22,
        delay: 0,
    },
    {
        text: "語彙",
        x: 88,
        y: 9,
        size: 1,
        blur: 2.5,
        opacity: 0.17,
        delay: 1.8,
    },
    {
        text: "archive",
        x: 4,
        y: 52,
        size: 0.72,
        blur: 1.8,
        opacity: 0.19,
        delay: 3.2,
    },
    {
        text: "Frequenz",
        x: 88,
        y: 48,
        size: 0.7,
        blur: 2.4,
        opacity: 0.16,
        delay: 2.4,
    },
    {
        text: "signal",
        x: 9,
        y: 86,
        size: 0.85,
        blur: 2.6,
        opacity: 0.2,
        delay: 4.1,
        magenta: true,
    },
    {
        text: "εισαγωγή",
        x: 83,
        y: 82,
        size: 0.68,
        blur: 2.2,
        opacity: 0.17,
        delay: 1.2,
    },
    {
        text: "channel",
        x: 74,
        y: 91,
        size: 0.7,
        blur: 2.8,
        opacity: 0.16,
        delay: 2.8,
    },
    {
        text: "κλειδί",
        x: 14,
        y: 30,
        size: 0.65,
        blur: 1.6,
        opacity: 0.17,
        delay: 3.6,
    },
    {
        text: "entrada",
        x: 80,
        y: 28,
        size: 0.6,
        blur: 3,
        opacity: 0.16,
        delay: 0.6,
        magenta: true,
    },
    {
        text: "lexicon",
        x: 20,
        y: 93,
        size: 0.75,
        blur: 2.2,
        opacity: 0.17,
        delay: 4.6,
    },
    {
        text: "aperture",
        x: 91,
        y: 66,
        size: 0.62,
        blur: 2.6,
        opacity: 0.14,
        delay: 5.2,
    },
    {
        text: "access",
        x: 5,
        y: 70,
        size: 0.7,
        blur: 2.4,
        opacity: 0.17,
        delay: 1.5,
        magenta: true,
    },
];

type SignalApertureAuthBackgroundProps = {
    readonly children: ReactNode;
    readonly footerMark?: string;
};

export const SignalApertureAuthBackground = ({
    children,
    footerMark = "ƒ/1.4 · signal chamber · aperture",
}: SignalApertureAuthBackgroundProps) => (
    <main className={styles.page}>
        <div className={styles.ghostLayer} aria-hidden="true">
            {ghostWords.map((ghost, i) => (
                <span
                    key={`${ghost.text}-${i}`}
                    className={`${styles.ghostWord}${ghost.magenta ? ` ${styles.magentaGhost}` : ""}`}
                    style={
                        {
                            left: `${ghost.x}%`,
                            top: `${ghost.y}%`,
                            fontSize: `${ghost.size}rem`,
                            "--ghost-blur": `${ghost.blur}px`,
                            opacity: ghost.opacity,
                            animationDelay: `-${ghost.delay}s`,
                        } as CSSProperties
                    }
                >
                    {ghost.text}
                </span>
            ))}
        </div>

        <div className={styles.apertureBackdrop} aria-hidden="true">
            <ApertureRing className={styles.apertureRingLarge} />
        </div>

        <div className={styles.apertureBackdropSecondary} aria-hidden="true">
            <ApertureRing className={styles.apertureRingSecondary} />
        </div>

        <div className={styles.signalTraces} aria-hidden="true">
            <div className={styles.traceH1} />
            <div className={styles.traceH2} />
            <div className={styles.traceV} />
        </div>

        <div className={styles.radialGlow} aria-hidden="true" />
        <div className={styles.focalStage} aria-hidden="true" />

        {children}

        <p className={styles.pageFooterMark} aria-hidden="true">
            {footerMark}
        </p>
    </main>
);
