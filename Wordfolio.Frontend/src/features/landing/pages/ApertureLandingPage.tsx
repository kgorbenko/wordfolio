import { useEffect, useRef, useState } from "react";
import type { CSSProperties } from "react";
import { Link } from "@tanstack/react-router";

import { loginPath, registerPath } from "../../auth/routes";
import { ApertureRing } from "../components/aperture/ApertureRing";

import styles from "./ApertureLandingPage.module.scss";

type MorphEntry = {
    word: string;
    lang: string;
};

const morphSequence: MorphEntry[] = [
    { word: "word", lang: "English" },
    { word: "mot", lang: "Français" },
    { word: "Wort", lang: "Deutsch" },
    { word: "слово", lang: "Русский" },
    { word: "言葉", lang: "日本語" },
    { word: "단어", lang: "한국어" },
    { word: "词", lang: "中文" },
    { word: "λέξη", lang: "Ελληνικά" },
    { word: "كلمة", lang: "العربية" },
    { word: "palavra", lang: "Português" },
    { word: "kelime", lang: "Türkçe" },
    { word: "szó", lang: "Magyar" },
];

type GhostWord = {
    text: string;
    x: number;
    y: number;
    size: number;
    blur: number;
    opacity: number;
    delay: number;
    magenta?: boolean;
};

const ghostWords: GhostWord[] = [
    { text: "ephemeral", x: 7, y: 14, size: 0.85, blur: 2, opacity: 0.09, delay: 0 },
    { text: "語彙", x: 86, y: 11, size: 1.05, blur: 2.5, opacity: 0.07, delay: 1.8 },
    { text: "serendipity", x: 4, y: 50, size: 0.72, blur: 1.8, opacity: 0.07, delay: 3.2 },
    { text: "Wanderlust", x: 89, y: 46, size: 0.8, blur: 2.2, opacity: 0.06, delay: 2.4 },
    { text: "saudade", x: 10, y: 83, size: 0.9, blur: 2.8, opacity: 0.07, delay: 4.1, magenta: true },
    { text: "詩", x: 84, y: 80, size: 1.1, blur: 2, opacity: 0.08, delay: 1.2 },
    { text: "parole", x: 74, y: 90, size: 0.75, blur: 2.5, opacity: 0.06, delay: 2.8 },
    { text: "τέχνη", x: 15, y: 32, size: 0.7, blur: 1.6, opacity: 0.07, delay: 3.6 },
    { text: "скрижаль", x: 80, y: 30, size: 0.62, blur: 3, opacity: 0.05, delay: 0.6, magenta: true },
    { text: "florilegio", x: 22, y: 92, size: 0.78, blur: 2.2, opacity: 0.06, delay: 4.6 },
    { text: "Zeitgeist", x: 90, y: 64, size: 0.68, blur: 2.4, opacity: 0.05, delay: 5.2 },
    { text: "murmure", x: 6, y: 68, size: 0.74, blur: 2.6, opacity: 0.06, delay: 1.5, magenta: true },
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

    return (
        <main className={styles.page}>
            <div className={styles.ghostLayer} aria-hidden="true">
                {ghostWords.map((ghost, i) => (
                    <span
                        key={i}
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

            <div className={styles.apertureStage}>
                <ApertureRing className={styles.apertureRing} />

                <div className={styles.morphContainer}>
                    <span className={morphClassName}>{current.word}</span>
                    <span className={langClassName}>{current.lang}</span>
                </div>
            </div>

            <div className={styles.content}>
                <p className={styles.brandName}>Wordfolio</p>
                <p className={styles.tagline}>Your vocabulary, in focus.</p>

                <div className={styles.ctaRow}>
                    <Link
                        {...loginPath()}
                        className={`${styles.cta} ${styles.ctaLogin}`}
                    >
                        Login
                    </Link>
                    <Link
                        {...registerPath()}
                        className={`${styles.cta} ${styles.ctaRegister}`}
                    >
                        Register
                    </Link>
                </div>
            </div>

            <p className={styles.footerMark} aria-hidden="true">
                ƒ/1.4 · ∞
            </p>
        </main>
    );
};
