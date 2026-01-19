import { useState, useEffect } from "react";
import {
    Box,
    TextField,
    Button,
    Typography,
    Paper,
    IconButton,
    Chip,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    alpha,
    useTheme,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";

import {
    DefinitionRequest,
    TranslationRequest,
    ExampleRequest,
} from "../../api/entriesApi";

interface ExampleItem {
    id: string;
    exampleText: string;
    source: "Api" | "Custom";
}

interface DefinitionItem {
    id: string;
    definitionText: string;
    source: "Api" | "Manual";
    examples: ExampleItem[];
}

interface TranslationItem {
    id: string;
    translationText: string;
    source: "Api" | "Manual";
    examples: ExampleItem[];
}

export interface EntryFormValues {
    readonly entryText: string;
    readonly definitions: DefinitionItem[];
    readonly translations: TranslationItem[];
}

export interface EntryFormOutput {
    readonly entryText: string;
    readonly definitions: DefinitionRequest[];
    readonly translations: TranslationRequest[];
}

interface EntryFormProps {
    readonly defaultValues?: EntryFormValues;
    readonly onSubmit: (data: EntryFormOutput) => void;
    readonly onCancel: () => void;
    readonly submitLabel: string;
    readonly isLoading?: boolean;
    readonly showEntryText?: boolean;
    readonly showFooter?: boolean;
    readonly onChange?: (data: EntryFormOutput, isValid: boolean) => void;
}

export const EntryForm = ({
    defaultValues,
    onSubmit,
    onCancel,
    submitLabel,
    isLoading = false,
    showEntryText = true,
    showFooter = true,
    onChange,
}: EntryFormProps) => {
    const theme = useTheme();

    const [entryText, setEntryText] = useState(defaultValues?.entryText ?? "");
    const [definitions, setDefinitions] = useState<DefinitionItem[]>(
        defaultValues?.definitions ?? []
    );
    const [translations, setTranslations] = useState<TranslationItem[]>(
        defaultValues?.translations ?? []
    );

    const [newDefinitionText, setNewDefinitionText] = useState("");
    const [isAddDefinitionOpen, setIsAddDefinitionOpen] = useState(false);

    const [newTranslationText, setNewTranslationText] = useState("");
    const [isAddTranslationOpen, setIsAddTranslationOpen] = useState(false);

    const [newExampleText, setNewExampleText] = useState("");
    const [addExampleFor, setAddExampleFor] = useState<{
        type: "definition" | "translation";
        id: string;
    } | null>(null);

    const handleDefinitionChange = (defId: string, value: string) => {
        setDefinitions((prev) =>
            prev.map((d) =>
                d.id === defId ? { ...d, definitionText: value } : d
            )
        );
    };

    const handleDeleteDefinition = (defId: string) => {
        setDefinitions((prev) => prev.filter((d) => d.id !== defId));
    };

    const handleAddDefinition = () => {
        if (newDefinitionText.trim()) {
            const newId = `def-${Date.now()}`;
            setDefinitions((prev) => [
                ...prev,
                {
                    id: newId,
                    definitionText: newDefinitionText.trim(),
                    source: "Manual",
                    examples: [],
                },
            ]);
            setNewDefinitionText("");
            setIsAddDefinitionOpen(false);
        }
    };

    const handleTranslationChange = (transId: string, value: string) => {
        setTranslations((prev) =>
            prev.map((t) =>
                t.id === transId ? { ...t, translationText: value } : t
            )
        );
    };

    const handleDeleteTranslation = (transId: string) => {
        setTranslations((prev) => prev.filter((t) => t.id !== transId));
    };

    const handleAddTranslation = () => {
        if (newTranslationText.trim()) {
            const newId = `trans-${Date.now()}`;
            setTranslations((prev) => [
                ...prev,
                {
                    id: newId,
                    translationText: newTranslationText.trim(),
                    source: "Manual",
                    examples: [],
                },
            ]);
            setNewTranslationText("");
            setIsAddTranslationOpen(false);
        }
    };

    const handleDeleteExample = (
        type: "definition" | "translation",
        parentId: string,
        exampleId: string
    ) => {
        if (type === "definition") {
            setDefinitions((prev) =>
                prev.map((d) =>
                    d.id === parentId
                        ? {
                              ...d,
                              examples: d.examples.filter(
                                  (ex) => ex.id !== exampleId
                              ),
                          }
                        : d
                )
            );
        } else {
            setTranslations((prev) =>
                prev.map((t) =>
                    t.id === parentId
                        ? {
                              ...t,
                              examples: t.examples.filter(
                                  (ex) => ex.id !== exampleId
                              ),
                          }
                        : t
                )
            );
        }
    };

    const handleAddExample = () => {
        if (addExampleFor && newExampleText.trim()) {
            const newExampleId = `ex-${Date.now()}`;
            const newExample: ExampleItem = {
                id: newExampleId,
                exampleText: newExampleText.trim(),
                source: "Custom",
            };

            if (addExampleFor.type === "definition") {
                setDefinitions((prev) =>
                    prev.map((d) =>
                        d.id === addExampleFor.id
                            ? { ...d, examples: [...d.examples, newExample] }
                            : d
                    )
                );
            } else {
                setTranslations((prev) =>
                    prev.map((t) =>
                        t.id === addExampleFor.id
                            ? { ...t, examples: [...t.examples, newExample] }
                            : t
                    )
                );
            }
            setNewExampleText("");
            setAddExampleFor(null);
        }
    };

    const handleSubmit = () => {
        const definitionRequests: DefinitionRequest[] = definitions.map(
            (d) => ({
                definitionText: d.definitionText,
                source: d.source,
                examples: d.examples.map(
                    (ex): ExampleRequest => ({
                        exampleText: ex.exampleText,
                        source: ex.source,
                    })
                ),
            })
        );

        const translationRequests: TranslationRequest[] = translations.map(
            (t) => ({
                translationText: t.translationText,
                source: t.source,
                examples: t.examples.map(
                    (ex): ExampleRequest => ({
                        exampleText: ex.exampleText,
                        source: ex.source,
                    })
                ),
            })
        );

        onSubmit({
            entryText: entryText.trim(),
            definitions: definitionRequests,
            translations: translationRequests,
        });
    };

    const hasContent = definitions.length > 0 || translations.length > 0;
    const isValid = entryText.trim().length > 0 && hasContent;

    useEffect(() => {
        if (onChange) {
            const definitionRequests: DefinitionRequest[] = definitions.map(
                (d) => ({
                    definitionText: d.definitionText,
                    source: d.source,
                    examples: d.examples.map(
                        (ex): ExampleRequest => ({
                            exampleText: ex.exampleText,
                            source: ex.source,
                        })
                    ),
                })
            );

            const translationRequests: TranslationRequest[] = translations.map(
                (t) => ({
                    translationText: t.translationText,
                    source: t.source,
                    examples: t.examples.map(
                        (ex): ExampleRequest => ({
                            exampleText: ex.exampleText,
                            source: ex.source,
                        })
                    ),
                })
            );

            onChange(
                {
                    entryText: entryText.trim(),
                    definitions: definitionRequests,
                    translations: translationRequests,
                },
                hasContent
            );
        }
    }, [entryText, definitions, translations, hasContent, onChange]);

    return (
        <Box sx={{ maxWidth: 800 }}>
            {showEntryText && (
                <TextField
                    autoFocus
                    fullWidth
                    label="Word or Phrase"
                    value={entryText}
                    onChange={(e) => setEntryText(e.target.value)}
                    disabled={isLoading}
                    sx={{ mb: 4 }}
                />
            )}

            <Box sx={{ mb: 4 }}>
                <Box
                    sx={{
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "space-between",
                        mb: 2,
                    }}
                >
                    <Typography
                        variant="h6"
                        fontWeight={600}
                        sx={{ color: "primary.main" }}
                    >
                        Definitions
                    </Typography>
                    <Button
                        size="small"
                        startIcon={<AddIcon />}
                        onClick={() => setIsAddDefinitionOpen(true)}
                        disabled={isLoading}
                    >
                        Add
                    </Button>
                </Box>
                {definitions.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                        No definitions yet
                    </Typography>
                ) : (
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 2,
                        }}
                    >
                        {definitions.map((def, index) => (
                            <Paper
                                key={def.id}
                                variant="outlined"
                                sx={{
                                    p: 2.5,
                                    borderColor: alpha(
                                        theme.palette.primary.main,
                                        0.2
                                    ),
                                }}
                            >
                                <Box
                                    sx={{
                                        display: "flex",
                                        alignItems: "flex-start",
                                        gap: 2,
                                    }}
                                >
                                    <Chip
                                        label={index + 1}
                                        size="small"
                                        color="primary"
                                        sx={{ fontWeight: 600, minWidth: 28 }}
                                    />
                                    <Box sx={{ flex: 1 }}>
                                        <Box
                                            sx={{
                                                display: "flex",
                                                alignItems: "flex-start",
                                                gap: 1,
                                            }}
                                        >
                                            <TextField
                                                fullWidth
                                                multiline
                                                value={def.definitionText}
                                                onChange={(e) =>
                                                    handleDefinitionChange(
                                                        def.id,
                                                        e.target.value
                                                    )
                                                }
                                                size="small"
                                                disabled={isLoading}
                                            />
                                            <IconButton
                                                size="small"
                                                onClick={() =>
                                                    handleDeleteDefinition(
                                                        def.id
                                                    )
                                                }
                                                sx={{ color: "error.main" }}
                                                disabled={isLoading}
                                            >
                                                <DeleteIcon fontSize="small" />
                                            </IconButton>
                                        </Box>
                                        {def.examples.length > 0 && (
                                            <Box sx={{ mt: 2 }}>
                                                {def.examples.map((ex) => (
                                                    <Box
                                                        key={ex.id}
                                                        sx={{
                                                            display: "flex",
                                                            alignItems:
                                                                "flex-start",
                                                            gap: 1,
                                                            mt: 1,
                                                            pl: 1,
                                                            borderLeft: `2px solid ${alpha(theme.palette.primary.main, 0.3)}`,
                                                        }}
                                                    >
                                                        <FormatQuoteIcon
                                                            sx={{
                                                                fontSize: 16,
                                                                color: "text.secondary",
                                                                mt: 0.25,
                                                            }}
                                                        />
                                                        <Typography
                                                            variant="body2"
                                                            color="text.secondary"
                                                            sx={{
                                                                fontStyle:
                                                                    "italic",
                                                                flex: 1,
                                                            }}
                                                        >
                                                            {ex.exampleText}
                                                        </Typography>
                                                        <IconButton
                                                            size="small"
                                                            onClick={() =>
                                                                handleDeleteExample(
                                                                    "definition",
                                                                    def.id,
                                                                    ex.id
                                                                )
                                                            }
                                                            sx={{
                                                                color: "error.main",
                                                            }}
                                                            disabled={isLoading}
                                                        >
                                                            <DeleteIcon fontSize="small" />
                                                        </IconButton>
                                                    </Box>
                                                ))}
                                            </Box>
                                        )}
                                        <Button
                                            size="small"
                                            startIcon={<AddIcon />}
                                            onClick={() =>
                                                setAddExampleFor({
                                                    type: "definition",
                                                    id: def.id,
                                                })
                                            }
                                            sx={{ mt: 1 }}
                                            disabled={isLoading}
                                        >
                                            Add Example
                                        </Button>
                                    </Box>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                )}
            </Box>

            <Box sx={{ mb: 4 }}>
                <Box
                    sx={{
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "space-between",
                        mb: 2,
                    }}
                >
                    <Typography
                        variant="h6"
                        fontWeight={600}
                        sx={{ color: "secondary.main" }}
                    >
                        Translations
                    </Typography>
                    <Button
                        size="small"
                        startIcon={<AddIcon />}
                        onClick={() => setIsAddTranslationOpen(true)}
                        disabled={isLoading}
                    >
                        Add
                    </Button>
                </Box>
                {translations.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                        No translations yet
                    </Typography>
                ) : (
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 2,
                        }}
                    >
                        {translations.map((trans, index) => (
                            <Paper
                                key={trans.id}
                                variant="outlined"
                                sx={{
                                    p: 2.5,
                                    borderColor: alpha(
                                        theme.palette.secondary.main,
                                        0.2
                                    ),
                                }}
                            >
                                <Box
                                    sx={{
                                        display: "flex",
                                        alignItems: "flex-start",
                                        gap: 2,
                                    }}
                                >
                                    <Chip
                                        label={index + 1}
                                        size="small"
                                        color="secondary"
                                        sx={{ fontWeight: 600, minWidth: 28 }}
                                    />
                                    <Box sx={{ flex: 1 }}>
                                        <Box
                                            sx={{
                                                display: "flex",
                                                alignItems: "flex-start",
                                                gap: 1,
                                            }}
                                        >
                                            <TextField
                                                fullWidth
                                                multiline
                                                value={trans.translationText}
                                                onChange={(e) =>
                                                    handleTranslationChange(
                                                        trans.id,
                                                        e.target.value
                                                    )
                                                }
                                                size="small"
                                                disabled={isLoading}
                                            />
                                            <IconButton
                                                size="small"
                                                onClick={() =>
                                                    handleDeleteTranslation(
                                                        trans.id
                                                    )
                                                }
                                                sx={{ color: "error.main" }}
                                                disabled={isLoading}
                                            >
                                                <DeleteIcon fontSize="small" />
                                            </IconButton>
                                        </Box>
                                        {trans.examples.length > 0 && (
                                            <Box sx={{ mt: 2 }}>
                                                {trans.examples.map((ex) => (
                                                    <Box
                                                        key={ex.id}
                                                        sx={{
                                                            display: "flex",
                                                            alignItems:
                                                                "flex-start",
                                                            gap: 1,
                                                            mt: 1,
                                                            pl: 1,
                                                            borderLeft: `2px solid ${alpha(theme.palette.secondary.main, 0.3)}`,
                                                        }}
                                                    >
                                                        <FormatQuoteIcon
                                                            sx={{
                                                                fontSize: 16,
                                                                color: "text.secondary",
                                                                mt: 0.25,
                                                            }}
                                                        />
                                                        <Typography
                                                            variant="body2"
                                                            color="text.secondary"
                                                            sx={{
                                                                fontStyle:
                                                                    "italic",
                                                                flex: 1,
                                                            }}
                                                        >
                                                            {ex.exampleText}
                                                        </Typography>
                                                        <IconButton
                                                            size="small"
                                                            onClick={() =>
                                                                handleDeleteExample(
                                                                    "translation",
                                                                    trans.id,
                                                                    ex.id
                                                                )
                                                            }
                                                            sx={{
                                                                color: "error.main",
                                                            }}
                                                            disabled={isLoading}
                                                        >
                                                            <DeleteIcon fontSize="small" />
                                                        </IconButton>
                                                    </Box>
                                                ))}
                                            </Box>
                                        )}
                                        <Button
                                            size="small"
                                            startIcon={<AddIcon />}
                                            onClick={() =>
                                                setAddExampleFor({
                                                    type: "translation",
                                                    id: trans.id,
                                                })
                                            }
                                            sx={{ mt: 1 }}
                                            disabled={isLoading}
                                        >
                                            Add Example
                                        </Button>
                                    </Box>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                )}
            </Box>

            {showFooter && (
                <>
                    {!hasContent && (
                        <Typography
                            variant="body2"
                            color="error"
                            sx={{ mb: 2, textAlign: "right" }}
                        >
                            At least one definition or translation is required
                        </Typography>
                    )}

                    <Box
                        sx={{
                            display: "flex",
                            gap: 2,
                            justifyContent: "flex-end",
                        }}
                    >
                        <Button onClick={onCancel} disabled={isLoading}>
                            Cancel
                        </Button>
                        <Button
                            variant="contained"
                            onClick={handleSubmit}
                            disabled={isLoading || !isValid}
                        >
                            {isLoading ? "Saving..." : submitLabel}
                        </Button>
                    </Box>
                </>
            )}

            <Dialog
                open={isAddDefinitionOpen}
                onClose={() => setIsAddDefinitionOpen(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>Add Definition</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        multiline
                        rows={3}
                        label="Definition"
                        value={newDefinitionText}
                        onChange={(e) => setNewDefinitionText(e.target.value)}
                        sx={{ mt: 1 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setIsAddDefinitionOpen(false)}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        onClick={handleAddDefinition}
                        disabled={!newDefinitionText.trim()}
                    >
                        Add
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={isAddTranslationOpen}
                onClose={() => setIsAddTranslationOpen(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>Add Translation</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        label="Translation"
                        value={newTranslationText}
                        onChange={(e) => setNewTranslationText(e.target.value)}
                        sx={{ mt: 1 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setIsAddTranslationOpen(false)}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        onClick={handleAddTranslation}
                        disabled={!newTranslationText.trim()}
                    >
                        Add
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={addExampleFor !== null}
                onClose={() => setAddExampleFor(null)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>Add Example</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        multiline
                        rows={2}
                        label="Example sentence"
                        value={newExampleText}
                        onChange={(e) => setNewExampleText(e.target.value)}
                        sx={{ mt: 1 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setAddExampleFor(null)}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        onClick={handleAddExample}
                        disabled={!newExampleText.trim()}
                    >
                        Add
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};
