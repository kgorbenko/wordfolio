import { useCallback } from "react";
import {
    Box,
    Typography,
    IconButton,
    useMediaQuery,
    useTheme,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";

import type { CreateEntryData } from "../../api/types/entries";
import { EntryLookupForm, VocabularyContext } from "./EntryLookupForm";
import { ResponsiveDialog } from "../ResponsiveDialog";
import styles from "./WordEntrySheet.module.scss";

interface WordEntrySheetProps {
    readonly open: boolean;
    readonly initialVocabularyId?: string;
    readonly isSaving: boolean;
    readonly onClose: () => void;
    readonly onSave: (
        context: VocabularyContext | null,
        request: CreateEntryData
    ) => void;
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

    const handleSave = useCallback(
        (context: VocabularyContext | null, request: CreateEntryData) => {
            onSave(context, request);
        },
        [onSave]
    );

    const content = (
        <Box
            className={styles.container}
            sx={{
                height: isMobile ? "85vh" : "auto",
                maxHeight: isMobile ? "85vh" : "80vh",
            }}
        >
            <Box className={styles.header}>
                <Typography variant="h6">Add Word</Typography>
                <IconButton onClick={onClose} size="small">
                    <CloseIcon />
                </IconButton>
            </Box>

            {open && (
                <EntryLookupForm
                    vocabularyId={initialVocabularyId}
                    showVocabularySelector={true}
                    isSaving={isSaving}
                    onSave={handleSave}
                    onCancel={onClose}
                    onLookupError={onLookupError}
                    autoFocus={true}
                />
            )}
        </Box>
    );

    return (
        <ResponsiveDialog
            open={open}
            onClose={onClose}
            maxWidth="md"
            fullWidth
            dialogPaperClassName={styles.dialog}
        >
            {content}
        </ResponsiveDialog>
    );
};
