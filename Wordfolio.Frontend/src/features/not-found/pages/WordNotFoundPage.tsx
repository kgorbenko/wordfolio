import { Button } from "@mui/material";
import { Link } from "@tanstack/react-router";

import { homePath } from "../../auth/routes";
import { missingCollectionPreviewPath, notFoundPreviewPath } from "../routes";

import styles from "./WordNotFoundPage.module.scss";

const fragments = [
    { text: "§", left: "8%", top: "15%", delay: "0s", duration: "14s" },
    { text: "◊", left: "85%", top: "25%", delay: "3s", duration: "11s" },
    { text: "¶", left: "72%", top: "70%", delay: "7s", duration: "16s" },
    { text: "†", left: "18%", top: "80%", delay: "5s", duration: "13s" },
    { text: "∅", left: "50%", top: "10%", delay: "9s", duration: "15s" },
    { text: "·", left: "35%", top: "90%", delay: "2s", duration: "10s" },
];

export const WordNotFoundPage = () => (
    <div className={styles.page}>
        <div className={styles.fragments} aria-hidden="true">
            {fragments.map((f) => (
                <span
                    key={f.text}
                    className={styles.fragment}
                    style={{
                        left: f.left,
                        top: f.top,
                        animationDelay: f.delay,
                        animationDuration: f.duration,
                    }}
                >
                    {f.text}
                </span>
            ))}
        </div>

        <article className={styles.card}>
            <div className={styles.headwordRow}>
                <h1 className={styles.headword}>
                    404
                    <span className={styles.cursor} aria-hidden="true" />
                </h1>
                <span className={styles.phonetics}>/nɒt.faʊnd/</span>
                <span className={styles.partOfSpeech}>n.</span>
            </div>

            <div className={styles.meta}>
                <span className={styles.badge}>Computing</span>
                <div
                    className={styles.frequencyDots}
                    title="Frequency: disturbingly common"
                >
                    <span className={`${styles.dot} ${styles.dotActive}`} />
                    <span className={`${styles.dot} ${styles.dotActive}`} />
                    <span className={`${styles.dot} ${styles.dotActive}`} />
                    <span className={`${styles.dot} ${styles.dotActive}`} />
                    <span className={styles.dot} />
                </div>
            </div>

            <hr className={styles.divider} />

            <div className={styles.definitionSection}>
                <p className={styles.definition}>
                    <span className={styles.senseNumber}>1</span>
                    The state of seeking a page that does not, has never, and
                    will never exist at this address — a digital cul-de-sac, if
                    you will.
                </p>
            </div>

            <div className={styles.exampleSection}>
                <p className={styles.example}>
                    "She typed the URL with great confidence, only to be greeted
                    by <span className={styles.exampleHighlight}>a 404</span>{" "}
                    and a quiet sense of existential drift."
                </p>
            </div>

            <div className={styles.synonyms}>
                <span className={styles.synonymsLabel}>cf.</span>
                <span className={styles.synonymTag}>gone</span>
                <span className={styles.synonymTag}>missing</span>
                <span className={styles.synonymTag}>void</span>
                <span className={styles.synonymTag}>¯\_(ツ)_/¯</span>
            </div>

            <div className={styles.actions}>
                <Button
                    component={Link}
                    {...notFoundPreviewPath()}
                    variant="contained"
                    className={styles.primaryAction}
                >
                    Back to Showcase
                </Button>
                <Button
                    component={Link}
                    {...homePath()}
                    variant="outlined"
                    className={styles.secondaryAction}
                >
                    Go Home
                </Button>
                <Button
                    component={Link}
                    {...missingCollectionPreviewPath()}
                    variant="outlined"
                    className={styles.secondaryAction}
                >
                    Missing Collection
                </Button>
            </div>

            <p className={styles.edition} aria-hidden="true">
                Wordfolio Unabridged · 2025 ed.
            </p>
        </article>
    </div>
);
