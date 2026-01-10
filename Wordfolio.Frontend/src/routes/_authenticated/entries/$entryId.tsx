import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import {
    Box,
    Container,
    Typography,
    IconButton,
    Paper,
    Chip,
    Divider,
    Button,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    alpha,
    useTheme,
} from "@mui/material";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import SaveIcon from "@mui/icons-material/Save";
import CancelIcon from "@mui/icons-material/Cancel";
import AddIcon from "@mui/icons-material/Add";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";

import { useNotificationContext } from "../../../contexts/NotificationContext";

interface Example {
    id: number;
    exampleText: string;
}

interface Definition {
    id: number;
    definitionText: string;
    examples: Example[];
}

interface Translation {
    id: number;
    translationText: string;
    examples: Example[];
}

interface Entry {
    id: number;
    entryText: string;
    createdAt: string;
    updatedAt: string | null;
    definitions: Definition[];
    translations: Translation[];
}

const stubEntries: Entry[] = [
    {
        id: 1,
        entryText: "serendipity",
        createdAt: "2026-01-08T10:30:00Z",
        updatedAt: null,
        definitions: [
            {
                id: 1,
                definitionText:
                    "The occurrence and development of events by chance in a happy or beneficial way",
                examples: [
                    {
                        id: 1,
                        exampleText:
                            "A fortunate stroke of serendipity brought them together.",
                    },
                    {
                        id: 2,
                        exampleText:
                            "The discovery was made by pure serendipity.",
                    },
                ],
            },
            {
                id: 2,
                definitionText:
                    "Good luck in making unexpected and fortunate discoveries",
                examples: [],
            },
        ],
        translations: [
            {
                id: 1,
                translationText: "счастливая случайность",
                examples: [
                    {
                        id: 3,
                        exampleText:
                            "Это была счастливая случайность — This was serendipity",
                    },
                ],
            },
            {
                id: 2,
                translationText: "интуитивная прозорливость",
                examples: [],
            },
        ],
    },
    {
        id: 2,
        entryText: "ephemeral",
        createdAt: "2026-01-07T14:20:00Z",
        updatedAt: "2026-01-09T08:15:00Z",
        definitions: [
            {
                id: 3,
                definitionText: "Lasting for a very short time",
                examples: [
                    {
                        id: 4,
                        exampleText:
                            "Fame in the fashion industry is ephemeral.",
                    },
                ],
            },
        ],
        translations: [
            {
                id: 3,
                translationText: "мимолетный",
                examples: [],
            },
            {
                id: 4,
                translationText: "недолговечный",
                examples: [],
            },
        ],
    },
    {
        id: 3,
        entryText: "ubiquitous",
        createdAt: "2026-01-06T09:00:00Z",
        updatedAt: null,
        definitions: [
            {
                id: 4,
                definitionText: "Present, appearing, or found everywhere",
                examples: [
                    {
                        id: 5,
                        exampleText:
                            "Smartphones have become ubiquitous in modern society.",
                    },
                ],
            },
        ],
        translations: [
            {
                id: 5,
                translationText: "вездесущий",
                examples: [],
            },
            {
                id: 6,
                translationText: "повсеместный",
                examples: [],
            },
        ],
    },
];

const EntryDetailPage = () => {
    const theme = useTheme();
    const { entryId } = Route.useParams();
    const navigate = useNavigate();
    const { openSuccessNotification } = useNotificationContext();

    const stubEntry = stubEntries.find((e) => e.id === Number(entryId));
    const [entry, setEntry] = useState<Entry | null>(stubEntry ?? null);
    const [isEditMode, setIsEditMode] = useState(false);
    const [editedEntry, setEditedEntry] = useState<Entry | null>(null);

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

    const [newDefinitionText, setNewDefinitionText] = useState("");
    const [isAddDefinitionOpen, setIsAddDefinitionOpen] = useState(false);

    const [newTranslationText, setNewTranslationText] = useState("");
    const [isAddTranslationOpen, setIsAddTranslationOpen] = useState(false);

    const [newExampleText, setNewExampleText] = useState("");
    const [addExampleFor, setAddExampleFor] = useState<{
        type: "definition" | "translation";
        id: number;
    } | null>(null);

    const handleStartEdit = () => {
        setEditedEntry(entry ? JSON.parse(JSON.stringify(entry)) : null);
        setIsEditMode(true);
    };

    const handleCancelEdit = () => {
        setEditedEntry(null);
        setIsEditMode(false);
    };

    const handleSaveEdit = () => {
        if (editedEntry) {
            setEntry({
                ...editedEntry,
                updatedAt: new Date().toISOString(),
            });
            openSuccessNotification({ message: "Entry updated" });
        }
        setIsEditMode(false);
        setEditedEntry(null);
    };

    const handleDelete = () => {
        openSuccessNotification({ message: "Entry deleted" });
        navigate({ to: "/collections" });
    };

    const handleEntryTextChange = (value: string) => {
        if (editedEntry) {
            setEditedEntry({ ...editedEntry, entryText: value });
        }
    };

    const handleDefinitionChange = (defId: number, value: string) => {
        if (editedEntry) {
            setEditedEntry({
                ...editedEntry,
                definitions: editedEntry.definitions.map((d) =>
                    d.id === defId ? { ...d, definitionText: value } : d
                ),
            });
        }
    };

    const handleDeleteDefinition = (defId: number) => {
        if (editedEntry) {
            setEditedEntry({
                ...editedEntry,
                definitions: editedEntry.definitions.filter(
                    (d) => d.id !== defId
                ),
            });
        }
    };

    const handleAddDefinition = () => {
        if (editedEntry && newDefinitionText.trim()) {
            const newId =
                Math.max(...editedEntry.definitions.map((d) => d.id), 0) + 1;
            setEditedEntry({
                ...editedEntry,
                definitions: [
                    ...editedEntry.definitions,
                    {
                        id: newId,
                        definitionText: newDefinitionText.trim(),
                        examples: [],
                    },
                ],
            });
            setNewDefinitionText("");
            setIsAddDefinitionOpen(false);
        }
    };

    const handleTranslationChange = (transId: number, value: string) => {
        if (editedEntry) {
            setEditedEntry({
                ...editedEntry,
                translations: editedEntry.translations.map((t) =>
                    t.id === transId ? { ...t, translationText: value } : t
                ),
            });
        }
    };

    const handleDeleteTranslation = (transId: number) => {
        if (editedEntry) {
            setEditedEntry({
                ...editedEntry,
                translations: editedEntry.translations.filter(
                    (t) => t.id !== transId
                ),
            });
        }
    };

    const handleAddTranslation = () => {
        if (editedEntry && newTranslationText.trim()) {
            const newId =
                Math.max(...editedEntry.translations.map((t) => t.id), 0) + 1;
            setEditedEntry({
                ...editedEntry,
                translations: [
                    ...editedEntry.translations,
                    {
                        id: newId,
                        translationText: newTranslationText.trim(),
                        examples: [],
                    },
                ],
            });
            setNewTranslationText("");
            setIsAddTranslationOpen(false);
        }
    };

    const handleDeleteExample = (
        type: "definition" | "translation",
        parentId: number,
        exampleId: number
    ) => {
        if (editedEntry) {
            if (type === "definition") {
                setEditedEntry({
                    ...editedEntry,
                    definitions: editedEntry.definitions.map((d) =>
                        d.id === parentId
                            ? {
                                ...d,
                                examples: d.examples.filter(
                                    (ex) => ex.id !== exampleId
                                ),
                            }
                            : d
                    ),
                });
            } else {
                setEditedEntry({
                    ...editedEntry,
                    translations: editedEntry.translations.map((t) =>
                        t.id === parentId
                            ? {
                                ...t,
                                examples: t.examples.filter(
                                    (ex) => ex.id !== exampleId
                                ),
                            }
                            : t
                    ),
                });
            }
        }
    };

    const handleAddExample = () => {
        if (editedEntry && addExampleFor && newExampleText.trim()) {
            const newExampleId = Date.now();

            if (addExampleFor.type === "definition") {
                setEditedEntry({
                    ...editedEntry,
                    definitions: editedEntry.definitions.map((d) =>
                        d.id === addExampleFor.id
                            ? {
                                ...d,
                                examples: [
                                    ...d.examples,
                                    {
                                        id: newExampleId,
                                        exampleText: newExampleText.trim(),
                                    },
                                ],
                            }
                            : d
                    ),
                });
            } else {
                setEditedEntry({
                    ...editedEntry,
                    translations: editedEntry.translations.map((t) =>
                        t.id === addExampleFor.id
                            ? {
                                ...t,
                                examples: [
                                    ...t.examples,
                                    {
                                        id: newExampleId,
                                        exampleText: newExampleText.trim(),
                                    },
                                ],
                            }
                            : t
                    ),
                });
            }
            setNewExampleText("");
            setAddExampleFor(null);
        }
    };

    const currentEntry = isEditMode ? editedEntry : entry;

    if (!currentEntry) {
        return (
            <Container maxWidth={false} sx={{ py: 4, maxWidth: 800 }}>
                <Typography variant="h5" color="text.secondary">
                    Entry not found
                </Typography>
            </Container>
        );
    }

    return (
        <Container maxWidth={false} sx={{ py: 4, maxWidth: 800 }}>
            <Box
                sx={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                    mb: 4,
                }}
            >
                <Box
                    sx={{
                        display: "flex",
                        alignItems: "center",
                        gap: 2,
                        flex: 1,
                    }}
                >
                    <IconButton
                        onClick={() => navigate({ to: "/collections" })}
                    >
                        <ArrowBackIcon />
                    </IconButton>
                    {isEditMode ? (
                        <TextField
                            value={currentEntry.entryText}
                            onChange={(e) =>
                                handleEntryTextChange(e.target.value)
                            }
                            variant="outlined"
                            sx={{
                                flex: 1,
                                "& .MuiOutlinedInput-input": {
                                    fontSize: "2rem",
                                    fontWeight: 700,
                                    py: 1,
                                },
                            }}
                        />
                    ) : (
                        <Typography variant="h4" fontWeight={700}>
                            {currentEntry.entryText}
                        </Typography>
                    )}
                </Box>
                <Box sx={{ display: "flex", gap: 1 }}>
                    {isEditMode ? (
                        <>
                            <IconButton onClick={handleCancelEdit}>
                                <CancelIcon />
                            </IconButton>
                            <IconButton
                                onClick={handleSaveEdit}
                                sx={{ color: "primary.main" }}
                            >
                                <SaveIcon />
                            </IconButton>
                        </>
                    ) : (
                        <>
                            <IconButton onClick={handleStartEdit}>
                                <EditIcon />
                            </IconButton>
                            <IconButton
                                onClick={() => setIsDeleteDialogOpen(true)}
                                sx={{
                                    color: "error.main",
                                    "&:hover": {
                                        bgcolor: alpha(
                                            theme.palette.error.main,
                                            0.1
                                        ),
                                    },
                                }}
                            >
                                <DeleteIcon />
                            </IconButton>
                        </>
                    )}
                </Box>
            </Box>

            {currentEntry.definitions.length > 0 && (
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
                        {isEditMode && (
                            <Button
                                size="small"
                                startIcon={<AddIcon />}
                                onClick={() => setIsAddDefinitionOpen(true)}
                            >
                                Add
                            </Button>
                        )}
                    </Box>
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 2,
                        }}
                    >
                        {currentEntry.definitions.map((def, index) => (
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
                                        {isEditMode ? (
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
                                                />
                                                <IconButton
                                                    size="small"
                                                    onClick={() =>
                                                        handleDeleteDefinition(
                                                            def.id
                                                        )
                                                    }
                                                    sx={{ color: "error.main" }}
                                                >
                                                    <DeleteIcon fontSize="small" />
                                                </IconButton>
                                            </Box>
                                        ) : (
                                            <Typography
                                                variant="body1"
                                                sx={{ lineHeight: 1.6 }}
                                            >
                                                {def.definitionText}
                                            </Typography>
                                        )}
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
                                                        {isEditMode && (
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
                                                            >
                                                                <DeleteIcon fontSize="small" />
                                                            </IconButton>
                                                        )}
                                                    </Box>
                                                ))}
                                            </Box>
                                        )}
                                        {isEditMode && (
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
                                            >
                                                Add Example
                                            </Button>
                                        )}
                                    </Box>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                </Box>
            )}

            {isEditMode && currentEntry.definitions.length === 0 && (
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
                        >
                            Add
                        </Button>
                    </Box>
                    <Typography variant="body2" color="text.secondary">
                        No definitions yet
                    </Typography>
                </Box>
            )}

            {currentEntry.translations.length > 0 && (
                <Box>
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
                        {isEditMode && (
                            <Button
                                size="small"
                                startIcon={<AddIcon />}
                                onClick={() => setIsAddTranslationOpen(true)}
                            >
                                Add
                            </Button>
                        )}
                    </Box>
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 2,
                        }}
                    >
                        {currentEntry.translations.map((trans, index) => (
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
                                        {isEditMode ? (
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
                                                    value={
                                                        trans.translationText
                                                    }
                                                    onChange={(e) =>
                                                        handleTranslationChange(
                                                            trans.id,
                                                            e.target.value
                                                        )
                                                    }
                                                    size="small"
                                                />
                                                <IconButton
                                                    size="small"
                                                    onClick={() =>
                                                        handleDeleteTranslation(
                                                            trans.id
                                                        )
                                                    }
                                                    sx={{ color: "error.main" }}
                                                >
                                                    <DeleteIcon fontSize="small" />
                                                </IconButton>
                                            </Box>
                                        ) : (
                                            <Typography
                                                variant="body1"
                                                sx={{ lineHeight: 1.6 }}
                                            >
                                                {trans.translationText}
                                            </Typography>
                                        )}
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
                                                        {isEditMode && (
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
                                                            >
                                                                <DeleteIcon fontSize="small" />
                                                            </IconButton>
                                                        )}
                                                    </Box>
                                                ))}
                                            </Box>
                                        )}
                                        {isEditMode && (
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
                                            >
                                                Add Example
                                            </Button>
                                        )}
                                    </Box>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                </Box>
            )}

            {isEditMode && currentEntry.translations.length === 0 && (
                <Box>
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
                        >
                            Add
                        </Button>
                    </Box>
                    <Typography variant="body2" color="text.secondary">
                        No translations yet
                    </Typography>
                </Box>
            )}

            <Divider sx={{ my: 4 }} />

            <Box sx={{ display: "flex", justifyContent: "center" }}>
                <Typography variant="caption" color="text.secondary">
                    Added{" "}
                    {new Date(currentEntry.createdAt).toLocaleDateString()}
                    {currentEntry.updatedAt &&
                        ` · Updated ${new Date(currentEntry.updatedAt).toLocaleDateString()}`}
                </Typography>
            </Box>

            <Dialog
                open={isDeleteDialogOpen}
                onClose={() => setIsDeleteDialogOpen(false)}
            >
                <DialogTitle>Delete Entry</DialogTitle>
                <DialogContent>
                    <Typography>
                        Are you sure you want to delete &quot;
                        {currentEntry.entryText}
                        &quot;? This action cannot be undone.
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setIsDeleteDialogOpen(false)}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        color="error"
                        onClick={handleDelete}
                    >
                        Delete
                    </Button>
                </DialogActions>
            </Dialog>

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
        </Container>
    );
};

export const Route = createFileRoute("/_authenticated/entries/$entryId")({
    component: EntryDetailPage,
});
