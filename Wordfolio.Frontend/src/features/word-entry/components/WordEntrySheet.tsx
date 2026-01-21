import { useState, useEffect, useRef, useCallback, useMemo } from "react";
import {
    Drawer,
    Dialog,
    Box,
    Typography,
    IconButton,
    Button,
    CircularProgress,
    useMediaQuery,
    useTheme,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";

import { CreateEntryRequest } from "../../../api/entriesApi";
import { useCollectionsHierarchyQuery } from "../../../queries/useCollectionsHierarchyQuery";
import {
    EntryFormHandle,
    EntryFormOutput,
} from "../../entries/components/EntryForm";
import { useWordLookup } from "../hooks/useWordLookup";
import { VocabularySelector } from "./VocabularySelector";
import { WordLookupInput } from "./WordLookupInput";
import { LookupResultsSection } from "./LookupResultsSection";
import styles from "./WordEntrySheet.module.scss";

interface WordEntrySheetProps {
    readonly open: boolean;
    readonly initialVocabularyId?: number;
    readonly isSaving: boolean;
    readonly onClose: () => void;
    readonly onSave: (request: CreateEntryRequest) => void;
    readonly onLookupError?: (message: string) => void;
}

export const WordEntrySheet = ({
    open,
    initialVocabularyId,
    isSaving,
    onClose,
    onSave,
    onLookupError,
}: WordEntrySheetProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    const [selectedVocabularyId, setSelectedVocabularyId] = useState<number>(
        initialVocabularyId ?? 0
    );

    const inputRef = useRef<HTMLInputElement>(null);
    const entryFormRef = useRef<EntryFormHandle>(null);

    const { data: hierarchy } = useCollectionsHierarchyQuery();

    const lookupOptions = useMemo(
        () => ({ onError: onLookupError }),
        [onLookupError]
    );
    const { word, lookupState, setWord, clear, reset } =
        useWordLookup(lookupOptions);

    useEffect(() => {
        if (open) {
            setTimeout(() => inputRef.current?.focus(), 100);
        }
    }, [open]);

    useEffect(() => {
        if (!open) {
            reset();
        }
    }, [open, reset]);

    const handleClear = useCallback(() => {
        clear();
        inputRef.current?.focus();
    }, [clear]);

    const handleFormSubmit = useCallback(
        (data: EntryFormOutput) => {
            onSave({
                vocabularyId:
                    selectedVocabularyId === 0 ? null : selectedVocabularyId,
                entryText: word.trim(),
                definitions: data.definitions,
                translations: data.translations,
            });
        },
        [onSave, selectedVocabularyId, word]
    );

    const canSave =
        !isSaving &&
        lookupState.status === "success" &&
        word.trim().length >= 2;

    const content = (
        <Box
            className={styles.container}
            sx={{
                height: isMobile ? "85vh" : "auto",
                maxHeight: isMobile ? "85vh" : "80vh",
            }}
        >
            <Box className={styles.header}>
                <Typography variant="h6" fontWeight={600}>
                    Add Word
                </Typography>
                <IconButton onClick={onClose} size="small">
                    <CloseIcon />
                </IconButton>
            </Box>

            <Box className={styles.inputSection}>
                <VocabularySelector
                    value={selectedVocabularyId}
                    hierarchy={hierarchy}
                    onChange={setSelectedVocabularyId}
                />
                <WordLookupInput
                    ref={inputRef}
                    value={word}
                    onChange={setWord}
                    onClear={handleClear}
                />
            </Box>

            <Box className={styles.content}>
                <LookupResultsSection
                    word={word}
                    lookupState={lookupState}
                    entryFormRef={entryFormRef}
                    isSaving={isSaving}
                    onSubmit={handleFormSubmit}
                    onCancel={onClose}
                />
            </Box>

            <Box className={styles.footer}>
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
            </Box>
        </Box>
    );

    if (isMobile) {
        return (
            <Drawer
                anchor="bottom"
                open={open}
                onClose={onClose}
                PaperProps={{ className: styles.mobileDrawer }}
            >
                {content}
            </Drawer>
        );
    }

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="sm"
            fullWidth
            PaperProps={{ className: styles.dialog }}
        >
            {content}
        </Dialog>
    );
};
