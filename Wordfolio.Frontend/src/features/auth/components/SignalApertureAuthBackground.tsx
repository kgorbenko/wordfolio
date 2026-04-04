import type { ReactNode } from "react";

import { ApertureRing } from "../../../shared/components/aperture/ApertureRing";

import styles from "./SignalApertureAuthBackground.module.scss";

type SignalApertureAuthBackgroundProps = {
    readonly children: ReactNode;
};

export const SignalApertureAuthBackground = ({
    children,
}: SignalApertureAuthBackgroundProps) => (
    <main className={styles.page}>
        <div className={styles.apertureBackdrop} aria-hidden="true">
            <ApertureRing className={styles.apertureRingLarge} />
        </div>

        <div className={styles.apertureBackdropSecondary} aria-hidden="true">
            <ApertureRing className={styles.apertureRingSecondary} />
        </div>

        <div className={styles.signalTraces} aria-hidden="true">
            <div className={styles.traceV} />
        </div>

        <div className={styles.radialGlow} aria-hidden="true" />
        <div className={styles.focalStage} aria-hidden="true" />

        {children}
    </main>
);
