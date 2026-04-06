import { useEffect, useRef, useState } from "react";
import { Link } from "@tanstack/react-router";
import { Button, Typography } from "@mui/material";

import { loginPath, registerPath } from "../../auth/routes";
import { ApertureRing } from "../../../shared/components/aperture/ApertureRing";

import styles from "./ApertureLandingPage.module.scss";

type MorphEntry = {
    word: string;
    lang: string;
    hint: string;
};

const morphSequence: MorphEntry[] = [
    { word: "word", lang: "English", hint: "the smallest unit of meaning" },
    { word: "mot", lang: "Français", hint: "le pouvoir de nommer" },
    { word: "Wort", lang: "Deutsch", hint: "Sprache formt Gedanken" },
    { word: "слово", lang: "Русский", hint: "начало всякой мысли" },
    { word: "言葉", lang: "日本語", hint: "意味を宿すもの" },
    { word: "단어", lang: "한국어", hint: "생각을 담는 그릇" },
    { word: "词", lang: "中文", hint: "字里行间的力量" },
    { word: "λέξη", lang: "Ελληνικά", hint: "η αρχή του λόγου" },
    { word: "كلمة", lang: "العربية", hint: "أصل كلّ بيان" },
    { word: "palavra", lang: "Português", hint: "a matéria da memória" },
    { word: "kelime", lang: "Türkçe", hint: "dilin en küçük parçası" },
    { word: "szó", lang: "Magyar", hint: "a gondolat hangja" },
];

const MORPH_INTERVAL_MS = 3200;
const EXIT_DURATION_MS = 480;

type MorphPhase = "visible" | "exiting" | "entering";

export const ApertureLandingPage = () => {
    const [wordIndex, setWordIndex] = useState(0);
    const [phase, setPhase] = useState<MorphPhase>("visible");
    const rafRef = useRef(0);

    useEffect(() => {
        const timer = setInterval(() => {
            setPhase("exiting");

            setTimeout(() => {
                setWordIndex((prev) => (prev + 1) % morphSequence.length);
                setPhase("entering");

                rafRef.current = requestAnimationFrame(() => {
                    rafRef.current = requestAnimationFrame(() => {
                        setPhase("visible");
                    });
                });
            }, EXIT_DURATION_MS);
        }, MORPH_INTERVAL_MS);

        return () => {
            clearInterval(timer);
            cancelAnimationFrame(rafRef.current);
        };
    }, []);

    const current = morphSequence[wordIndex];

    const morphClassName = [
        styles.morphWord,
        phase === "exiting" ? styles.morphExiting : "",
        phase === "entering" ? styles.morphEntering : "",
    ]
        .filter(Boolean)
        .join(" ");

    const langClassName = [
        styles.morphLang,
        phase !== "visible" ? styles.morphLangHidden : "",
    ]
        .filter(Boolean)
        .join(" ");

    const hintClassName = [
        styles.morphHint,
        phase !== "visible" ? styles.morphHintHidden : "",
    ]
        .filter(Boolean)
        .join(" ");

    return (
        <main className={styles.page}>
            <div className={styles.apertureStage}>
                <ApertureRing className={styles.apertureRing} />

                <div className={styles.morphContainer}>
                    <span className={morphClassName}>{current.word}</span>
                    <span className={langClassName}>{current.lang}</span>
                    <span className={hintClassName}>{current.hint}</span>
                </div>
            </div>

            <div className={styles.bridge} aria-hidden="true">
                <span className={styles.bridgeLine} />
                <span className={styles.bridgeLabel}>lexicon · focused</span>
                <span className={styles.bridgeLine} />
            </div>

            <div className={styles.content}>
                <Typography
                    component="p"
                    color="text.secondary"
                    className={styles.brandName}
                >
                    Wordfolio
                </Typography>
                <Typography
                    component="p"
                    color="text.secondary"
                    className={styles.tagline}
                >
                    A private archive for every word worth remembering — across
                    every language you carry.
                </Typography>

                <div className={styles.ctaRow}>
                    <Button
                        component={Link as React.ElementType}
                        {...loginPath()}
                        variant="contained"
                        color="primary"
                    >
                        Sign in
                    </Button>
                    <Button
                        component={Link as React.ElementType}
                        {...registerPath()}
                        variant="outlined"
                        color="primary"
                    >
                        Create archive
                    </Button>
                </div>

                <ul className={styles.pillRow} aria-label="Features">
                    <li className={styles.pill}>Entries · living</li>
                    <li className={styles.pill}>Context · preserved</li>
                    <li className={styles.pill}>Recall · intentional</li>
                </ul>
            </div>

            <p className={styles.footerMark} aria-hidden="true">
                ƒ/1.4 · {morphSequence.length} languages · ∞
            </p>
        </main>
    );
};
