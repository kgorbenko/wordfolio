import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { useState } from "react";
import {
    Box,
    Container,
    Typography,
    Fab,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Button,
    Breadcrumbs,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { VocabularyCard } from "../../../../components/vocabularies/VocabularyCard";
import { EmptyState } from "../../../../components/common/EmptyState";

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
    entryCount: number;
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

const initialVocabularies: Vocabulary[] = [
    {
        id: 1,
        collectionId: 1,
        name: "Catcher in the Rye",
        description: "J.D. Salinger's classic",
        entryCount: 42,
    },
    {
        id: 2,
        collectionId: 1,
        name: "1984",
        description: "George Orwell's dystopia",
        entryCount: 28,
    },
    {
        id: 3,
        collectionId: 1,
        name: "To Kill a Mockingbird",
        description: "Harper Lee",
        entryCount: 15,
    },
    {
        id: 4,
        collectionId: 2,
        name: "The Shawshank Redemption",
        description: null,
        entryCount: 33,
    },
    {
        id: 5,
        collectionId: 2,
        name: "Pulp Fiction",
        description: "Tarantino's masterpiece",
        entryCount: 21,
    },
    {
        id: 6,
        collectionId: 3,
        name: "Technical Terms",
        description: "Software development jargon",
        entryCount: 67,
    },
    {
        id: 7,
        collectionId: 3,
        name: "Business English",
        description: "Corporate vocabulary",
        entryCount: 45,
    },
    {
        id: 8,
        collectionId: 4,
        name: "My Words",
        description: "Default vocabulary for quick word entries",
        entryCount: 12,
    },
];

const CollectionDetailPage = () => {
    const { collectionId } = Route.useParams();
    const navigate = useNavigate();
    const numericCollectionId = Number(collectionId);

    const collection = stubCollections.find(
        (c) => c.id === numericCollectionId
    );
    const [vocabularies, setVocabularies] = useState<Vocabulary[]>(
        initialVocabularies.filter(
            (v) => v.collectionId === numericCollectionId
        )
    );

    const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
    const [newVocabularyName, setNewVocabularyName] = useState("");
    const [newVocabularyDescription, setNewVocabularyDescription] =
        useState("");

    const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
    const [editingVocabulary, setEditingVocabulary] =
        useState<Vocabulary | null>(null);
    const [editName, setEditName] = useState("");
    const [editDescription, setEditDescription] = useState("");

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [deletingVocabulary, setDeletingVocabulary] =
        useState<Vocabulary | null>(null);

    const handleCreateVocabulary = () => {
        if (!newVocabularyName.trim()) return;

        const newVocabulary: Vocabulary = {
            id: Math.max(...vocabularies.map((v) => v.id), 0) + 100,
            collectionId: numericCollectionId,
            name: newVocabularyName.trim(),
            description: newVocabularyDescription.trim() || null,
            entryCount: 0,
        };
        setVocabularies((prev) => [...prev, newVocabulary]);
        setIsCreateDialogOpen(false);
        setNewVocabularyName("");
        setNewVocabularyDescription("");
    };

    const handleEditClick = (vocabulary: Vocabulary) => {
        setEditingVocabulary(vocabulary);
        setEditName(vocabulary.name);
        setEditDescription(vocabulary.description || "");
        setIsEditDialogOpen(true);
    };

    const handleEditSave = () => {
        if (!editingVocabulary || !editName.trim()) return;

        setVocabularies((prev) =>
            prev.map((v) =>
                v.id === editingVocabulary.id
                    ? {
                        ...v,
                        name: editName.trim(),
                        description: editDescription.trim() || null,
                    }
                    : v
            )
        );
        setIsEditDialogOpen(false);
        setEditingVocabulary(null);
    };

    const handleDeleteClick = (vocabulary: Vocabulary) => {
        setDeletingVocabulary(vocabulary);
        setIsDeleteDialogOpen(true);
    };

    const handleDeleteConfirm = () => {
        if (!deletingVocabulary) return;

        setVocabularies((prev) =>
            prev.filter((v) => v.id !== deletingVocabulary.id)
        );
        setIsDeleteDialogOpen(false);
        setDeletingVocabulary(null);
    };

    if (!collection) {
        return (
            <Container maxWidth={false} sx={{ py: 4 }}>
                <Typography variant="h5" color="text.secondary">
                    Collection not found
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
                <Typography color="text.primary" fontWeight={500}>
                    {collection.name}
                </Typography>
            </Breadcrumbs>

            <Typography variant="h4" gutterBottom fontWeight={600}>
                {collection.name}
            </Typography>
            {collection.description && (
                <Typography
                    variant="body1"
                    color="text.secondary"
                    sx={{ mb: 3 }}
                >
                    {collection.description}
                </Typography>
            )}

            {vocabularies.length === 0 ? (
                <EmptyState
                    icon={
                        <MenuBookIcon
                            sx={{ fontSize: 40, color: "secondary.main" }}
                        />
                    }
                    title="No Vocabularies Yet"
                    description="Add your first vocabulary - a book, movie, or any source of new words."
                    actionLabel="Add Vocabulary"
                    onAction={() => setIsCreateDialogOpen(true)}
                />
            ) : (
                <Box
                    sx={{
                        display: "grid",
                        gridTemplateColumns: {
                            xs: "1fr",
                            sm: "1fr 1fr",
                            md: "1fr 1fr 1fr",
                        },
                        gap: 2,
                    }}
                >
                    {vocabularies.map((vocab) => (
                        <VocabularyCard
                            key={vocab.id}
                            id={vocab.id}
                            name={vocab.name}
                            description={vocab.description ?? undefined}
                            entryCount={vocab.entryCount}
                            onClick={() =>
                                void navigate({
                                    to: "/collections/$collectionId/$vocabularyId",
                                    params: {
                                        collectionId: String(collectionId),
                                        vocabularyId: String(vocab.id),
                                    },
                                })
                            }
                            onEdit={() => handleEditClick(vocab)}
                            onDelete={() => handleDeleteClick(vocab)}
                        />
                    ))}
                </Box>
            )}

            <Fab
                color="secondary"
                aria-label="Add vocabulary"
                onClick={() => setIsCreateDialogOpen(true)}
                sx={{
                    position: "fixed",
                    bottom: { xs: 140, md: 24 },
                    right: { xs: 24, md: 100 },
                }}
            >
                <AddIcon />
            </Fab>

            <Dialog
                open={isCreateDialogOpen}
                onClose={() => setIsCreateDialogOpen(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>Add Vocabulary</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        label="Name"
                        value={newVocabularyName}
                        onChange={(e) => setNewVocabularyName(e.target.value)}
                        sx={{ mt: 1, mb: 2 }}
                    />
                    <TextField
                        fullWidth
                        label="Description (optional)"
                        value={newVocabularyDescription}
                        onChange={(e) =>
                            setNewVocabularyDescription(e.target.value)
                        }
                        multiline
                        rows={2}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setIsCreateDialogOpen(false)}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        onClick={handleCreateVocabulary}
                        disabled={!newVocabularyName.trim()}
                    >
                        Add
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={isEditDialogOpen}
                onClose={() => setIsEditDialogOpen(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>Edit Vocabulary</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        label="Name"
                        value={editName}
                        onChange={(e) => setEditName(e.target.value)}
                        sx={{ mt: 1, mb: 2 }}
                    />
                    <TextField
                        fullWidth
                        label="Description (optional)"
                        value={editDescription}
                        onChange={(e) => setEditDescription(e.target.value)}
                        multiline
                        rows={2}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setIsEditDialogOpen(false)}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        onClick={handleEditSave}
                        disabled={!editName.trim()}
                    >
                        Save
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={isDeleteDialogOpen}
                onClose={() => setIsDeleteDialogOpen(false)}
            >
                <DialogTitle>Delete Vocabulary</DialogTitle>
                <DialogContent>
                    <Typography>
                        Are you sure you want to delete &quot;
                        {deletingVocabulary?.name}
                        &quot;? This will also delete all entries within it.
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setIsDeleteDialogOpen(false)}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        color="error"
                        onClick={handleDeleteConfirm}
                    >
                        Delete
                    </Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
};

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/"
)({
    component: CollectionDetailPage,
});
