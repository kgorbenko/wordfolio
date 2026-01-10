import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { useState } from "react";
import {
    Box,
    Container,
    Typography,
    Breadcrumbs,
    TextField,
    InputAdornment,
    alpha,
    useTheme,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { EntryListItem } from "../../../../components/entries/EntryListItem";
import { EmptyState } from "../../../../components/common/EmptyState";
import { useUiStore } from "../../../../stores/uiStore";

interface Collection {
    id: number;
    name: string;
    description: string | null;
}

interface Vocabulary {
    id: number;
    collectionId: number;
    name: string;
    description: string | null;
}

interface Entry {
    id: number;
    vocabularyId: number;
    entryText: string;
    firstDefinition: string | null;
    firstTranslation: string | null;
    createdAt: string;
}

const stubCollections: Collection[] = [
    { id: 1, name: "Books", description: "Words from books I'm reading" },
    {
        id: 2,
        name: "Movies",
        description: "Vocabulary from films and TV shows",
    },
    { id: 3, name: "Work", description: "Professional and technical terms" },
    {
        id: 4,
        name: "Unsorted",
        description: "Default collection for quick word entries",
    },
];

const stubVocabularies: Vocabulary[] = [
    {
        id: 1,
        collectionId: 1,
        name: "Catcher in the Rye",
        description: "J.D. Salinger's classic",
    },
    {
        id: 2,
        collectionId: 1,
        name: "1984",
        description: "George Orwell's dystopia",
    },
    {
        id: 3,
        collectionId: 1,
        name: "To Kill a Mockingbird",
        description: "Harper Lee",
    },
    {
        id: 4,
        collectionId: 2,
        name: "The Shawshank Redemption",
        description: null,
    },
    {
        id: 5,
        collectionId: 2,
        name: "Pulp Fiction",
        description: "Tarantino's masterpiece",
    },
    {
        id: 6,
        collectionId: 3,
        name: "Technical Terms",
        description: "Software development jargon",
    },
    {
        id: 7,
        collectionId: 3,
        name: "Business English",
        description: "Corporate vocabulary",
    },
    {
        id: 8,
        collectionId: 4,
        name: "My Words",
        description: "Default vocabulary for quick word entries",
    },
];

const stubEntries: Entry[] = [
    {
        id: 1,
        vocabularyId: 1,
        entryText: "serendipity",
        firstDefinition:
            "The occurrence and development of events by chance in a happy or beneficial way",
        firstTranslation: "счастливая случайность",
        createdAt: "2026-01-08T10:30:00Z",
    },
    {
        id: 2,
        vocabularyId: 1,
        entryText: "ephemeral",
        firstDefinition: "Lasting for a very short time",
        firstTranslation: "мимолетный",
        createdAt: "2026-01-07T14:20:00Z",
    },
    {
        id: 3,
        vocabularyId: 1,
        entryText: "ubiquitous",
        firstDefinition: "Present, appearing, or found everywhere",
        firstTranslation: "вездесущий",
        createdAt: "2026-01-06T09:00:00Z",
    },
    {
        id: 4,
        vocabularyId: 2,
        entryText: "totalitarian",
        firstDefinition:
            "Relating to a system of government that is centralized and dictatorial",
        firstTranslation: "тоталитарный",
        createdAt: "2026-01-05T11:00:00Z",
    },
    {
        id: 5,
        vocabularyId: 2,
        entryText: "doublethink",
        firstDefinition:
            "The acceptance of contrary opinions or beliefs at the same time",
        firstTranslation: "двоемыслие",
        createdAt: "2026-01-04T16:30:00Z",
    },
    {
        id: 6,
        vocabularyId: 6,
        entryText: "refactoring",
        firstDefinition:
            "Restructuring existing code without changing its external behavior",
        firstTranslation: "рефакторинг",
        createdAt: "2026-01-03T09:15:00Z",
    },
    {
        id: 7,
        vocabularyId: 8,
        entryText: "melancholy",
        firstDefinition:
            "A feeling of pensive sadness, typically with no obvious cause",
        firstTranslation: "меланхолия",
        createdAt: "2026-01-02T20:00:00Z",
    },
];

const VocabularyDetailPage = () => {
    const theme = useTheme();
    const { collectionId, vocabularyId } = Route.useParams();
    const navigate = useNavigate();
    const { openWordEntry } = useUiStore();
    const [searchQuery, setSearchQuery] = useState("");

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);

    const collection = stubCollections.find(
        (c) => c.id === numericCollectionId
    );
    const vocabulary = stubVocabularies.find(
        (v) => v.id === numericVocabularyId
    );
    const entries = stubEntries.filter(
        (e) => e.vocabularyId === numericVocabularyId
    );

    const filteredEntries = entries.filter((entry) =>
        entry.entryText.toLowerCase().includes(searchQuery.toLowerCase())
    );

    if (!collection || !vocabulary) {
        return (
            <Container maxWidth={false} sx={{ py: 4 }}>
                <Typography variant="h5" color="text.secondary">
                    Vocabulary not found
                </Typography>
            </Container>
        );
    }

    return (
        <Container maxWidth={false} sx={{ py: 4 }}>
            <Breadcrumbs
                separator={<NavigateNextIcon fontSize="small" />}
                sx={{ mb: 2 }}
            >
                <Link
                    to="/collections"
                    style={{ textDecoration: "none", color: "inherit" }}
                >
                    <Typography
                        color="text.secondary"
                        sx={{ "&:hover": { color: "primary.main" } }}
                    >
                        Collections
                    </Typography>
                </Link>
                <Link
                    to="/collections/$collectionId"
                    params={{ collectionId }}
                    style={{ textDecoration: "none", color: "inherit" }}
                >
                    <Typography
                        color="text.secondary"
                        sx={{ "&:hover": { color: "primary.main" } }}
                    >
                        {collection.name}
                    </Typography>
                </Link>
                <Typography color="text.primary" fontWeight={500}>
                    {vocabulary.name}
                </Typography>
            </Breadcrumbs>

            <Box
                sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    mb: 3,
                }}
            >
                <Box>
                    <Typography variant="h4" fontWeight={600}>
                        {vocabulary.name}
                    </Typography>
                    {vocabulary.description && (
                        <Typography variant="body1" color="text.secondary">
                            {vocabulary.description}
                        </Typography>
                    )}
                </Box>
                <Typography variant="body2" color="text.secondary">
                    {entries.length} {entries.length === 1 ? "word" : "words"}
                </Typography>
            </Box>

            {entries.length > 0 && (
                <TextField
                    fullWidth
                    placeholder="Search words..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    InputProps={{
                        startAdornment: (
                            <InputAdornment position="start">
                                <SearchIcon sx={{ color: "text.secondary" }} />
                            </InputAdornment>
                        ),
                    }}
                    sx={{
                        mb: 2,
                        "& .MuiOutlinedInput-root": {
                            bgcolor: alpha(theme.palette.primary.main, 0.04),
                        },
                    }}
                />
            )}

            {entries.length === 0 ? (
                <EmptyState
                    icon={
                        <MenuBookIcon
                            sx={{ fontSize: 40, color: "secondary.main" }}
                        />
                    }
                    title="No Words Yet"
                    description="Tap the + button to add your first word to this vocabulary."
                    actionLabel="Add Word"
                    onAction={() => openWordEntry(numericVocabularyId)}
                />
            ) : filteredEntries.length === 0 ? (
                <Box sx={{ textAlign: "center", py: 4 }}>
                    <Typography variant="body1" color="text.secondary">
                        No words match your search.
                    </Typography>
                </Box>
            ) : (
                <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
                    {filteredEntries.map((entry) => (
                        <EntryListItem
                            key={entry.id}
                            id={entry.id}
                            entryText={entry.entryText}
                            firstDefinition={entry.firstDefinition ?? undefined}
                            firstTranslation={
                                entry.firstTranslation ?? undefined
                            }
                            createdAt={entry.createdAt}
                            onClick={() =>
                                void navigate({
                                    to: "/entries/$entryId",
                                    params: { entryId: String(entry.id) },
                                })
                            }
                        />
                    ))}
                </Box>
            )}
        </Container>
    );
};

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/$vocabularyId"
)({
    component: VocabularyDetailPage,
});
