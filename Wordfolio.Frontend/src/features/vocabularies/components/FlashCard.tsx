import { Box, Paper, Typography } from "@mui/material";

import type { Entry } from "../../../shared/api/types/entries";
import { EntryDetailContent } from "../../../shared/components/entries/EntryDetailContent";

import styles from "./FlashCard.module.scss";

interface FlashCardProps {
    readonly entry: Entry;
    readonly isFlipped: boolean;
    readonly onFlip: () => void;
}

export const FlashCard = ({ entry, isFlipped, onFlip }: FlashCardProps) => {
    const handleKeyDown = (event: React.KeyboardEvent) => {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            onFlip();
        }
    };

    return (
        <Paper
            className={styles.card}
            onClick={onFlip}
            onKeyDown={handleKeyDown}
            role="button"
            tabIndex={0}
            aria-label={isFlipped ? "Card back" : "Card front — tap to reveal"}
            elevation={2}
        >
            {isFlipped ? (
                <Box className={styles.back}>
                    <EntryDetailContent entry={entry} />
                </Box>
            ) : (
                <Box className={styles.front}>
                    <Typography variant="h2">{entry.entryText}</Typography>
                    <Typography
                        variant="body2"
                        color="text.secondary"
                        className={styles.hint}
                    >
                        Tap to reveal
                    </Typography>
                </Box>
            )}
        </Paper>
    );
};
