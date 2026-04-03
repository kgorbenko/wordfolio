import { Link as RouterLink } from "@tanstack/react-router";
import { Link as MuiLink, Typography } from "@mui/material";

import { homePath } from "../../auth/routes";
import styles from "./LostInTranslationAtlasPage.module.scss";

type LanguageNode = {
    word: string;
    lang: string;
    x: number;
    y: number;
    floatDelay: number;
    opacity: number;
};

const languageNodes: LanguageNode[] = [
    { word: "perdu", lang: "FR", x: 9, y: 13, floatDelay: 0, opacity: 0.55 },
    {
        word: "verloren",
        lang: "DE",
        x: 76,
        y: 9,
        floatDelay: 1.4,
        opacity: 0.5,
    },
    {
        word: "perdido",
        lang: "ES",
        x: 16,
        y: 46,
        floatDelay: 0.7,
        opacity: 0.6,
    },
    {
        word: "smarrito",
        lang: "IT",
        x: 7,
        y: 74,
        floatDelay: 2.2,
        opacity: 0.45,
    },
    {
        word: "χαμένος",
        lang: "EL",
        x: 83,
        y: 77,
        floatDelay: 1.8,
        opacity: 0.5,
    },
    {
        word: "elveszett",
        lang: "HU",
        x: 88,
        y: 29,
        floatDelay: 1.0,
        opacity: 0.4,
    },
    { word: "迷失", lang: "ZH", x: 55, y: 84, floatDelay: 1.5, opacity: 0.6 },
    { word: "迷子", lang: "JA", x: 67, y: 21, floatDelay: 0.3, opacity: 0.55 },
    {
        word: "kayboldu",
        lang: "TR",
        x: 28,
        y: 72,
        floatDelay: 2.5,
        opacity: 0.5,
    },
    { word: "خسارة", lang: "AR", x: 47, y: 11, floatDelay: 1.2, opacity: 0.5 },
    {
        word: "пропало",
        lang: "RU",
        x: 92,
        y: 54,
        floatDelay: 0.5,
        opacity: 0.4,
    },
    {
        word: "verdwijnen",
        lang: "NL",
        x: 22,
        y: 88,
        floatDelay: 3.0,
        opacity: 0.35,
    },
];

type ConstellationLine = {
    x1: number;
    y1: number;
    x2: number;
    y2: number;
};

const constellationLines: ConstellationLine[] = [
    { x1: 9, y1: 13, x2: 67, y2: 21 },
    { x1: 76, y1: 9, x2: 67, y2: 21 },
    { x1: 67, y1: 21, x2: 88, y2: 29 },
    { x1: 88, y1: 29, x2: 83, y2: 77 },
    { x1: 83, y1: 77, x2: 55, y2: 84 },
    { x1: 28, y1: 72, x2: 55, y2: 84 },
    { x1: 16, y1: 46, x2: 28, y2: 72 },
    { x1: 7, y1: 74, x2: 28, y2: 72 },
    { x1: 9, y1: 13, x2: 16, y2: 46 },
    { x1: 47, y1: 11, x2: 76, y2: 9 },
    { x1: 47, y1: 11, x2: 9, y2: 13 },
    { x1: 88, y1: 29, x2: 92, y2: 54 },
    { x1: 7, y1: 74, x2: 22, y2: 88 },
];

const CompassRoseSvg = () => (
    <svg
        viewBox="0 0 16 16"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true"
        className={styles.compassRose}
    >
        <line
            x1="8"
            y1="1"
            x2="8"
            y2="6.5"
            stroke="currentColor"
            strokeWidth="0.9"
            strokeLinecap="round"
        />
        <line
            x1="8"
            y1="9.5"
            x2="8"
            y2="15"
            stroke="currentColor"
            strokeWidth="0.9"
            strokeLinecap="round"
        />
        <line
            x1="1"
            y1="8"
            x2="6.5"
            y2="8"
            stroke="currentColor"
            strokeWidth="0.9"
            strokeLinecap="round"
        />
        <line
            x1="9.5"
            y1="8"
            x2="15"
            y2="8"
            stroke="currentColor"
            strokeWidth="0.9"
            strokeLinecap="round"
        />
        <circle cx="8" cy="8" r="1.2" fill="currentColor" opacity="0.7" />
    </svg>
);

const GlobeSvg = () => (
    <svg
        viewBox="0 0 64 64"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        role="img"
        aria-label="Globe"
    >
        <circle
            cx="32"
            cy="32"
            r="28.5"
            stroke="#B5F507"
            strokeWidth="1.5"
            opacity="0.7"
        />
        <ellipse
            cx="32"
            cy="32"
            rx="28.5"
            ry="11"
            stroke="#B5F507"
            strokeWidth="1"
            opacity="0.3"
        />
        <ellipse
            cx="32"
            cy="18"
            rx="21"
            ry="8"
            stroke="#B5F507"
            strokeWidth="0.8"
            opacity="0.22"
        />
        <ellipse
            cx="32"
            cy="46"
            rx="21"
            ry="8"
            stroke="#B5F507"
            strokeWidth="0.8"
            opacity="0.22"
        />
        <path
            d="M 32 3.5 C 44 17 44 47 32 60.5"
            stroke="#B5F507"
            strokeWidth="1"
            opacity="0.3"
        />
        <path
            d="M 32 3.5 C 20 17 20 47 32 60.5"
            stroke="#B5F507"
            strokeWidth="1"
            opacity="0.3"
        />
        <line
            x1="3.5"
            y1="32"
            x2="60.5"
            y2="32"
            stroke="#B5F507"
            strokeWidth="0.8"
            opacity="0.22"
        />
    </svg>
);

export const LostInTranslationAtlasPage = () => {
    return (
        <div className={styles.page}>
            <svg
                className={styles.constellationSvg}
                viewBox="0 0 100 100"
                preserveAspectRatio="none"
                aria-hidden="true"
            >
                {constellationLines.map((line, i) => (
                    <line
                        key={i}
                        x1={line.x1}
                        y1={line.y1}
                        x2={line.x2}
                        y2={line.y2}
                        className={styles.constellationLine}
                    />
                ))}
            </svg>

            <div className={styles.nodesLayer} aria-hidden="true">
                {languageNodes.map((node, i) => (
                    <div
                        key={i}
                        className={styles.languageNode}
                        style={{
                            left: `${node.x}%`,
                            top: `${node.y}%`,
                            opacity: node.opacity,
                            animationDelay: `-${node.floatDelay}s`,
                        }}
                    >
                        <span className={styles.nodeWord}>{node.word}</span>
                        <span className={styles.nodeLang}>{node.lang}</span>
                    </div>
                ))}
            </div>

            <div className={styles.hero}>
                <div
                    className={styles.statusBadge}
                    aria-label="HTTP status 404 — page not found"
                >
                    <span className={styles.statusDot} aria-hidden="true" />
                    HTTP 404 · PAGE NOT FOUND
                </div>

                <div className={styles.globeWrapper}>
                    <GlobeSvg />
                </div>

                <div className={styles.display404} aria-label="Error 404">
                    <span className={styles.digit}>4</span>
                    <span className={styles.digitZero}>0</span>
                    <span className={styles.digit}>4</span>
                </div>

                <Typography className={styles.title} component="h1">
                    Lost in Translation
                </Typography>

                <Typography className={styles.subtitle}>
                    This page exists in no known language.
                    <br />
                    It may have drifted beyond the edge of the atlas.
                </Typography>

                <div className={styles.coordinatesRow} aria-hidden="true">
                    <CompassRoseSvg />
                    <p className={styles.coordinates}>
                        φ 40.404° N &nbsp;·&nbsp; λ 0.404° W
                    </p>
                </div>

                <div className={styles.homeLinkRow}>
                    <MuiLink
                        component={RouterLink}
                        {...homePath()}
                        className={styles.homeLink}
                    >
                        Return Home
                    </MuiLink>
                </div>

                <p className={styles.terraNota} aria-hidden="true">
                    TERRA INCOGNITA
                </p>
            </div>
        </div>
    );
};
