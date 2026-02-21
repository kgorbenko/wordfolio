import { useState, useRef, useCallback, useMemo, useEffect } from "react";
import { Box, Button, CircularProgress } from "@mui/material";

import { CreateEntryRequest } from "../api/entriesApi";
import { ensureNonNullable } from "../../../utils/misc";
import { useCollectionsHierarchyQuery } from "../../../queries/useCollectionsHierarchyQuery";
import { EntryFormHandle, EntryFormOutput } from "./EntryForm";
import { useWordLookup } from "../hooks/useWordLookup";
import { VocabularySelector } from "./VocabularySelector";
import { WordLookupInput } from "./WordLookupInput";
import { LookupResultsSection } from "./LookupResultsSection";
import styles from "./EntryLookupForm.module.scss";

type EntryLookupFormVariant = "modal" | "page";

export interface VocabularyContext {
    readonly collectionId: number;
    readonly vocabularyId: number;
}

interface EntryLookupFormProps {
    readonly vocabularyId?: number;
    readonly showVocabularySelector?: boolean;
    readonly isSaving: boolean;
    readonly onSave: (
        context: VocabularyContext | null,
        request: CreateEntryRequest
    ) => void;
    readonly onCancel: () => void;
    readonly onLookupError?: (message: string) => void;
    readonly autoFocus?: boolean;
    readonly variant?: EntryLookupFormVariant;
}

export const EntryLookupForm = ({
    vocabularyId,
    showVocabularySelector = false,
    isSaving,
    onSave,
    onCancel,
    onLookupError,
    autoFocus = false,
    variant = "modal",
}: EntryLookupFormProps) => {
    const [selectedVocabularyId, setSelectedVocabularyId] = useState<
        number | undefined
    >(vocabularyId);

    const inputRef = useRef<HTMLInputElement>(null);
    const entryFormRef = useRef<EntryFormHandle>(null);

    const { data: hierarchy } = useCollectionsHierarchyQuery();

    const lookupOptions = useMemo(
        () => ({ onError: onLookupError }),
        [onLookupError]
    );
    const { word, lookupState, setWord, clear } = useWordLookup(lookupOptions);

    useEffect(() => {
        if (autoFocus) {
            setTimeout(() => inputRef.current?.focus(), 100);
        }
    }, [autoFocus]);

    const handleClear = useCallback(() => {
        clear();
        inputRef.current?.focus();
    }, [clear]);

    const handleFormSubmit = useCallback(
        (data: EntryFormOutput) => {
            const effectiveVocabularyId = showVocabularySelector
                ? selectedVocabularyId
                : vocabularyId;

            const context = effectiveVocabularyId !== undefined
                ? {
                    collectionId: ensureNonNullable(
                        ensureNonNullable(hierarchy).collections.find((c) =>
                            c.vocabularies.some(
                                (v) => v.id === selectedVocabularyId
                            )
                        )
                    ).id,
                    vocabularyId: effectiveVocabularyId,
                }
                : null;

            onSave(context, {
                entryText: word.trim(),
                definitions: data.definitions,
                translations: data.translations,
            });
        },
        [onSave, showVocabularySelector, selectedVocabularyId, hierarchy, word]
    );

    const canSave =
        !isSaving &&
        lookupState.status === "success" &&
        word.trim().length >= 2;

    const isPageVariant = variant === "page";

    return (
        <Box
            className={isPageVariant ? styles.containerPage : styles.container}
        >
            <Box
                className={
                    isPageVariant
                        ? styles.inputSectionPage
                        : styles.inputSection
                }
            >
                {showVocabularySelector && (
                    <VocabularySelector
                        value={selectedVocabularyId}
                        hierarchy={hierarchy}
                        onChange={setSelectedVocabularyId}
                    />
                )}
                <WordLookupInput
                    ref={inputRef}
                    value={word}
                    onChange={setWord}
                    onClear={handleClear}
                />
            </Box>

            <Box
                className={isPageVariant ? styles.contentPage : styles.content}
            >
                <LookupResultsSection
                    word={word}
                    lookupState={lookupState}
                    entryFormRef={entryFormRef}
                    isSaving={isSaving}
                    onSubmit={handleFormSubmit}
                    onCancel={onCancel}
                />
            </Box>

            <Box className={isPageVariant ? styles.actionsPage : styles.footer}>
                {isPageVariant ? (
                    <>
                        <Button onClick={onCancel} disabled={isSaving}>
                            Cancel
                        </Button>
                        <Button
                            variant="contained"
                            onClick={() => entryFormRef.current?.submit()}
                            disabled={!canSave}
                        >
                            {isSaving ? "Saving..." : "Save"}
                        </Button>
                    </>
                ) : (
                    <Button
                        fullWidth
                        variant="contained"
                        size="large"
                        onClick={() => entryFormRef.current?.submit()}
                        disabled={!canSave}
                        className={styles.saveButton}
                    >
                        {isSaving ? (
                            <CircularProgress size={24} color="inherit" />
                        ) : (
                            "Save"
                        )}
                    </Button>
                )}
            </Box>
        </Box>
    );
};
