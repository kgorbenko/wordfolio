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
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import ClearIcon from "@mui/icons-material/Clear";

import "./WordEntrySheet.scss";

import { useNotificationContext } from "../../contexts/NotificationContext";
import { dictionaryApi, DictionaryResult } from "../../api/dictionaryApi";
import { vocabulariesApi, VocabularyResponse } from "../../api/vocabulariesApi";
import {
    entriesApi,
    DefinitionRequest,
    TranslationRequest,
} from "../../api/entriesApi";
import { DefinitionsSection, DefinitionItem } from "./DefinitionsSection";
import { TranslationsSection, TranslationItem } from "./TranslationsSection";

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
    const [isSaving, setIsSaving] = useState(false);
    const [vocabularies, setVocabularies] = useState<
        Array<VocabularyResponse & { collectionName: string }>
    >([]);
    const [selectedVocabularyId, setSelectedVocabularyId] = useState<number>(
        initialVocabularyId ?? 0
    );

    const [definitions, setDefinitions] = useState<DefinitionItem[]>([]);
    const [translations, setTranslations] = useState<TranslationItem[]>([]);
    const [streamingText, setStreamingText] = useState("");
    const [hasResults, setHasResults] = useState(false);

    const abortControllerRef = useRef<AbortController | null>(null);
    const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    const loadVocabularies = useCallback(async () => {
        try {
            const allVocabs = await vocabulariesApi.getAllVocabularies();
            setVocabularies(allVocabs);

            if (
                initialVocabularyId !== undefined &&
                selectedVocabularyId === 0
            ) {
                setSelectedVocabularyId(allVocabs[0].id);
            }
        } catch {
            const defaultVocab =
                await vocabulariesApi.getOrCreateDefaultVocabulary();
            setVocabularies([{ ...defaultVocab, collectionName: "Unsorted" }]);
            setSelectedVocabularyId(defaultVocab.id);
        }
    }, [initialVocabularyId, selectedVocabularyId]);

    useEffect(() => {
        if (open) {
            loadVocabularies();
            setTimeout(() => inputRef.current?.focus(), 100);
        }
    }, [open, loadVocabularies]);

    useEffect(() => {
        if (!open) {
            setWord("");
            setDefinitions([]);
            setTranslations([]);
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
            setDefinitions([]);
            setTranslations([]);
            setStreamingText("");
            setHasResults(false);

            await dictionaryApi.lookupStream(
                searchWord,
                {
                    onText: (text) => setStreamingText(text),
                    onResult: (result: DictionaryResult) => {
                        const defs: DefinitionItem[] = result.definitions.map(
                            (d, i) => ({
                                id: `def-${i}`,
                                definitionText: d.definition,
                                partOfSpeech: d.partOfSpeech,
                                selected: true,
                                examples: d.exampleSentences.map((ex, j) => ({
                                    id: `def-${i}-ex-${j}`,
                                    exampleText: ex,
                                    source: "Api" as const,
                                    selected: true,
                                })),
                            })
                        );

                        const trans: TranslationItem[] =
                            result.translations.map((t, i) => ({
                                id: `trans-${i}`,
                                translationText: t.translation,
                                partOfSpeech: t.partOfSpeech,
                                selected: true,
                                examples: t.examples.map((ex, j) => ({
                                    id: `trans-${i}-ex-${j}`,
                                    exampleText: `${ex.russian} — ${ex.english}`,
                                    source: "Api" as const,
                                    selected: true,
                                })),
                            }));

                        setDefinitions(defs);
                        setTranslations(trans);
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
            setDefinitions([]);
            setTranslations([]);
            setHasResults(false);
        }
    };

    const handleClear = () => {
        setWord("");
        setDefinitions([]);
        setTranslations([]);
        setStreamingText("");
        setHasResults(false);
        setIsLoading(false);
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }
        inputRef.current?.focus();
    };

    const getSelectedCount = () => {
        const selectedDefs = definitions.filter((d) => d.selected).length;
        const selectedTrans = translations.filter((t) => t.selected).length;
        return selectedDefs + selectedTrans;
    };

    const handleSave = async () => {
        const selectedDefs = definitions.filter((d) => d.selected);
        const selectedTrans = translations.filter((t) => t.selected);

        if (selectedDefs.length === 0 && selectedTrans.length === 0) {
            openErrorNotification({
                message: "Please select at least one definition or translation",
            });
            return;
        }

        setIsSaving(true);

        try {
            const definitionRequests: DefinitionRequest[] = selectedDefs.map(
                (d) => ({
                    definitionText: d.definitionText,
                    source: "Api",
                    examples: d.examples
                        .filter((ex) => ex.selected)
                        .map((ex) => ({
                            exampleText: ex.exampleText,
                            source: ex.source,
                        })),
                })
            );

            const translationRequests: TranslationRequest[] = selectedTrans.map(
                (t) => ({
                    translationText: t.translationText,
                    source: "Api",
                    examples: t.examples
                        .filter((ex) => ex.selected)
                        .map((ex) => ({
                            exampleText: ex.exampleText,
                            source: ex.source,
                        })),
                })
            );

            await entriesApi.createEntry({
                vocabularyId:
                    selectedVocabularyId === 0 ? null : selectedVocabularyId,
                entryText: word.trim(),
                definitions: definitionRequests,
                translations: translationRequests,
            });

            openSuccessNotification({ message: "Added to vocabulary" });
            onClose();
        } catch {
            openErrorNotification({ message: "Failed to save entry" });
        } finally {
            setIsSaving(false);
        }
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
                        {vocabularies.map((v) => (
                            <MenuItem key={v.id} value={v.id}>
                                {v.collectionName} / {v.name}
                            </MenuItem>
                        ))}
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

                {hasResults && (
                    <>
                        <DefinitionsSection
                            definitions={definitions}
                            onChange={setDefinitions}
                        />
                        <TranslationsSection
                            translations={translations}
                            onChange={setTranslations}
                        />
                    </>
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
                        isSaving ||
                        isLoading ||
                        !hasResults ||
                        getSelectedCount() === 0
                    }
                    sx={{ py: 1.5 }}
                >
                    {isSaving ? (
                        <CircularProgress size={24} color="inherit" />
                    ) : (
                        `Save (${getSelectedCount()} items)`
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
