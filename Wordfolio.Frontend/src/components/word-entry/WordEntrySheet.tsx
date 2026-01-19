import { useState, useEffect, useCallback, useRef } from "react";
import {
    Drawer,
    Dialog,
    Box,
    Typography,
    IconButton,
    TextField,
    Button,
    CircularProgress,
    useMediaQuery,
    useTheme,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    InputAdornment,
    alpha,
    Paper,
    ListSubheader,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import ClearIcon from "@mui/icons-material/Clear";

import "./WordEntrySheet.scss";

import { useNotificationContext } from "../../contexts/NotificationContext";
import { dictionaryApi, DictionaryResult } from "../../api/dictionaryApi";
import { useCollectionsHierarchyQuery } from "../../queries/useCollectionsHierarchyQuery";
import { useCreateEntryMutation } from "../../mutations/useCreateEntryMutation";
import {
    EntryForm,
    EntryFormValues,
    EntryFormOutput,
} from "../entries/EntryForm";

interface WordEntrySheetProps {
    readonly open: boolean;
    readonly onClose: () => void;
    readonly initialVocabularyId?: number;
}

export const WordEntrySheet = ({
    open,
    onClose,
    initialVocabularyId,
}: WordEntrySheetProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();

    const [word, setWord] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [selectedVocabularyId, setSelectedVocabularyId] = useState<number>(
        initialVocabularyId ?? 0
    );

    const [formValues, setFormValues] = useState<EntryFormValues | null>(null);
    const [currentFormData, setCurrentFormData] =
        useState<EntryFormOutput | null>(null);
    const [isFormValid, setIsFormValid] = useState(false);
    const [streamingText, setStreamingText] = useState("");
    const [hasResults, setHasResults] = useState(false);

    const abortControllerRef = useRef<AbortController | null>(null);
    const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    const { data: hierarchy } = useCollectionsHierarchyQuery();

    const createEntryMutation = useCreateEntryMutation({
        onSuccess: () => {
            openSuccessNotification({ message: "Added to vocabulary" });
            onClose();
        },
        onError: () => {
            openErrorNotification({ message: "Failed to save entry" });
        },
    });

    useEffect(() => {
        if (open) {
            setTimeout(() => inputRef.current?.focus(), 100);
        }
    }, [open]);

    useEffect(() => {
        if (!open) {
            setWord("");
            setFormValues(null);
            setCurrentFormData(null);
            setIsFormValid(false);
            setStreamingText("");
            setHasResults(false);
            setIsLoading(false);
            if (abortControllerRef.current) {
                abortControllerRef.current.abort();
            }
            if (debounceTimerRef.current) {
                clearTimeout(debounceTimerRef.current);
            }
        }
    }, [open]);

    const performLookup = useCallback(
        async (searchWord: string) => {
            if (abortControllerRef.current) {
                abortControllerRef.current.abort();
            }

            abortControllerRef.current = new AbortController();
            setIsLoading(true);
            setFormValues(null);
            setCurrentFormData(null);
            setIsFormValid(false);
            setStreamingText("");
            setHasResults(false);

            await dictionaryApi.lookupStream(
                searchWord,
                {
                    onText: (text) => setStreamingText(text),
                    onResult: (result: DictionaryResult) => {
                        const values: EntryFormValues = {
                            entryText: searchWord,
                            definitions: result.definitions.map((d, i) => ({
                                id: `def-${i}`,
                                definitionText: d.definition,
                                source: "Api" as const,
                                examples: d.exampleSentences.map((ex, j) => ({
                                    id: `def-${i}-ex-${j}`,
                                    exampleText: ex,
                                    source: "Api" as const,
                                })),
                            })),
                            translations: result.translations.map((t, i) => ({
                                id: `trans-${i}`,
                                translationText: t.translation,
                                source: "Api" as const,
                                examples: t.examples.map((ex, j) => ({
                                    id: `trans-${i}-ex-${j}`,
                                    exampleText: `${ex.russian} — ${ex.english}`,
                                    source: "Api" as const,
                                })),
                            })),
                        };

                        setFormValues(values);
                        setHasResults(true);
                    },
                    onError: (error) => {
                        if (error.name !== "AbortError") {
                            openErrorNotification({
                                message: "Failed to look up word",
                            });
                        }
                    },
                    onComplete: () => setIsLoading(false),
                },
                abortControllerRef.current.signal
            );
        },
        [openErrorNotification]
    );

    const handleWordChange = (value: string) => {
        setWord(value);

        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
        }

        if (value.trim().length >= 2) {
            debounceTimerRef.current = setTimeout(() => {
                performLookup(value.trim());
            }, 500);
        } else {
            setFormValues(null);
            setCurrentFormData(null);
            setIsFormValid(false);
            setHasResults(false);
        }
    };

    const handleClear = () => {
        setWord("");
        setFormValues(null);
        setCurrentFormData(null);
        setIsFormValid(false);
        setStreamingText("");
        setHasResults(false);
        setIsLoading(false);
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }
        inputRef.current?.focus();
    };

    const handleFormChange = useCallback(
        (data: EntryFormOutput, isValid: boolean) => {
            setCurrentFormData(data);
            setIsFormValid(isValid);
        },
        []
    );

    const handleSave = () => {
        if (!currentFormData || !isFormValid) {
            openErrorNotification({
                message: "Please add at least one definition or translation",
            });
            return;
        }

        createEntryMutation.mutate({
            vocabularyId:
                selectedVocabularyId === 0 ? null : selectedVocabularyId,
            entryText: word.trim(),
            definitions: currentFormData.definitions,
            translations: currentFormData.translations,
        });
    };

    const content = (
        <Box
            className="container"
            sx={{
                height: isMobile ? "85vh" : "auto",
                maxHeight: isMobile ? "85vh" : "80vh",
            }}
        >
            <Box
                className="header"
                sx={{
                    borderBottom: 1,
                    borderColor: "divider",
                    bgcolor: "background.paper",
                }}
            >
                <Typography variant="h6" fontWeight={600}>
                    Add Word
                </Typography>
                <IconButton onClick={onClose} size="small">
                    <CloseIcon />
                </IconButton>
            </Box>

            <Box
                className="input-section"
                sx={{ borderBottom: 1, borderColor: "divider" }}
            >
                <FormControl fullWidth size="small" sx={{ mb: 2 }}>
                    <InputLabel>Vocabulary</InputLabel>
                    <Select<number>
                        value={selectedVocabularyId}
                        label="Vocabulary"
                        onChange={(e) => {
                            setSelectedVocabularyId(Number(e.target.value));
                        }}
                    >
                        <MenuItem value={0}>Drafts — organize later</MenuItem>
                        {hierarchy?.collections.map((collection) => [
                            <ListSubheader key={`header-${collection.id}`}>
                                {collection.name}
                            </ListSubheader>,
                            ...collection.vocabularies.map((vocab) => (
                                <MenuItem key={vocab.id} value={vocab.id}>
                                    {vocab.name}
                                </MenuItem>
                            )),
                        ])}
                    </Select>
                </FormControl>

                <TextField
                    inputRef={inputRef}
                    fullWidth
                    placeholder="Enter word or phrase..."
                    value={word}
                    onChange={(e) => handleWordChange(e.target.value)}
                    autoComplete="off"
                    InputProps={{
                        endAdornment: word && (
                            <InputAdornment position="end">
                                <IconButton
                                    onClick={handleClear}
                                    size="small"
                                    edge="end"
                                >
                                    <ClearIcon fontSize="small" />
                                </IconButton>
                            </InputAdornment>
                        ),
                    }}
                    sx={{
                        "& .MuiOutlinedInput-root": {
                            bgcolor: alpha(theme.palette.primary.main, 0.04),
                        },
                    }}
                />
            </Box>

            <Box className="content">
                {isLoading && !hasResults && streamingText && (
                    <Paper
                        className="streaming-paper"
                        variant="outlined"
                        sx={{
                            p: 2,
                            bgcolor: alpha(theme.palette.primary.main, 0.02),
                            borderColor: alpha(theme.palette.primary.main, 0.2),
                            color: "text.primary",
                        }}
                    >
                        {streamingText}
                        <Box
                            component="span"
                            className="streaming-cursor"
                            sx={{ bgcolor: "primary.main" }}
                        />
                    </Paper>
                )}

                {isLoading && !hasResults && !streamingText && (
                    <Box className="loading">
                        <CircularProgress size={20} />
                        <Typography variant="body2" color="text.secondary">
                            Looking up definitions...
                        </Typography>
                    </Box>
                )}

                {hasResults && formValues && (
                    <EntryForm
                        defaultValues={formValues}
                        onSubmit={handleSave}
                        onCancel={onClose}
                        submitLabel="Save"
                        isLoading={createEntryMutation.isPending}
                        showEntryText={false}
                        showFooter={false}
                        onChange={handleFormChange}
                    />
                )}

                {!isLoading && !hasResults && word.length >= 2 && (
                    <Box className="no-results">
                        <Typography variant="body2" color="text.secondary">
                            No results found. You can add definitions manually.
                        </Typography>
                    </Box>
                )}

                {!isLoading && !hasResults && word.length < 2 && (
                    <Box className="no-results">
                        <Typography variant="body2" color="text.secondary">
                            Type a word to look up definitions and translations
                        </Typography>
                    </Box>
                )}
            </Box>

            <Box
                className="footer"
                sx={{
                    borderTop: 1,
                    borderColor: "divider",
                    bgcolor: "background.paper",
                }}
            >
                <Button
                    fullWidth
                    variant="contained"
                    size="large"
                    onClick={handleSave}
                    disabled={
                        createEntryMutation.isPending ||
                        isLoading ||
                        !hasResults ||
                        !isFormValid
                    }
                    sx={{ py: 1.5 }}
                >
                    {createEntryMutation.isPending ? (
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
                PaperProps={{
                    className: "word-entry-sheet mobile-drawer",
                }}
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
            PaperProps={{
                className: "word-entry-sheet dialog",
            }}
        >
            {content}
        </Dialog>
    );
};
