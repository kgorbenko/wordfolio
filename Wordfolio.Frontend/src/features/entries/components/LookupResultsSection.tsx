import { RefObject } from "react";
import { Box, CircularProgress, Typography } from "@mui/material";

import { EntryForm, EntryFormHandle, EntryFormOutput } from "./EntryForm";
import { EntryFormValues, LookupState, WordLookupResult } from "../types";
import { LookupStreamingDisplay } from "./LookupStreamingDisplay";
import styles from "./LookupResultsSection.module.scss";

const toFormValues = (
    word: string,
    result: WordLookupResult
): EntryFormValues => ({
    entryText: word,
    definitions: result.definitions.map((d, i) => ({
        id: `def-${i}`,
        definitionText: d.text,
        source: "Api" as const,
        examples: d.examples.map((ex, j) => ({
            id: `def-${i}-ex-${j}`,
            exampleText: ex,
            source: "Api" as const,
        })),
    })),
    translations: result.translations.map((t, i) => ({
        id: `trans-${i}`,
        translationText: t.text,
        source: "Api" as const,
        examples: t.examples.map((ex, j) => ({
            id: `trans-${i}-ex-${j}`,
            exampleText: `${ex.russian} — ${ex.english}`,
            source: "Api" as const,
        })),
    })),
});

interface LookupResultsSectionProps {
    readonly word: string;
    readonly lookupState: LookupState;
    readonly entryFormRef: RefObject<EntryFormHandle | null>;
    readonly isSaving: boolean;
    readonly onSubmit: (data: EntryFormOutput) => void;
    readonly onCancel: () => void;
}

export const LookupResultsSection = ({
    word,
    lookupState,
    entryFormRef,
    isSaving,
    onSubmit,
    onCancel,
}: LookupResultsSectionProps) => {
    if (lookupState.status === "loading" && lookupState.streamingText) {
        return <LookupStreamingDisplay text={lookupState.streamingText} />;
    }

    if (lookupState.status === "loading" && !lookupState.streamingText) {
        return (
            <Box className={styles.loading}>
                <CircularProgress size={20} />
                <Typography variant="body2" color="text.secondary">
                    Looking up definitions...
                </Typography>
            </Box>
        );
    }

    if (lookupState.status === "success") {
        return (
            <EntryForm
                ref={entryFormRef}
                defaultValues={toFormValues(word, lookupState.result)}
                onSubmit={onSubmit}
                onCancel={onCancel}
                submitLabel="Save"
                isLoading={isSaving}
                showEntryText={false}
                showFooter={false}
            />
        );
    }

    if (lookupState.status === "empty" || lookupState.status === "error") {
        return (
            <Box className={styles.noResults}>
                <Typography variant="body2" color="text.secondary">
                    No results found.
                </Typography>
            </Box>
        );
    }

    if (lookupState.status === "idle" && word.length < 2) {
        return (
            <Box className={styles.noResults}>
                <Typography variant="body2" color="text.secondary">
                    Type a word to look up definitions and translations
                </Typography>
            </Box>
        );
    }

    return null;
};
