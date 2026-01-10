import { createFileRoute, useNavigate } from "@tanstack/react-router";
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
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import FolderIcon from "@mui/icons-material/Folder";

import { CollectionCard } from "../../../components/collections/CollectionCard";
import { EmptyState } from "../../../components/common/EmptyState";

interface Collection {
    id: number;
    name: string;
    description: string | null;
    vocabularyCount: number;
}

const initialCollections: Collection[] = [
    {
        id: 1,
        name: "Books",
        description: "Words from books I'm reading",
        vocabularyCount: 3,
    },
    {
        id: 2,
        name: "Movies",
        description: "Vocabulary from films and TV shows",
        vocabularyCount: 2,
    },
    {
        id: 3,
        name: "Work",
        description: "Professional and technical terms",
        vocabularyCount: 2,
    },
    {
        id: 4,
        name: "Unsorted",
        description: "Default collection for quick word entries",
        vocabularyCount: 1,
    },
];

const CollectionsPage = () => {
    const navigate = useNavigate();
    const [collections, setCollections] =
        useState<Collection[]>(initialCollections);

    const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
    const [newCollectionName, setNewCollectionName] = useState("");
    const [newCollectionDescription, setNewCollectionDescription] =
        useState("");

    const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
    const [editingCollection, setEditingCollection] =
        useState<Collection | null>(null);
    const [editName, setEditName] = useState("");
    const [editDescription, setEditDescription] = useState("");

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [deletingCollection, setDeletingCollection] =
        useState<Collection | null>(null);

    const handleCreateCollection = () => {
        if (!newCollectionName.trim()) return;

        const newCollection: Collection = {
            id: Math.max(...collections.map((c) => c.id), 0) + 1,
            name: newCollectionName.trim(),
            description: newCollectionDescription.trim() || null,
            vocabularyCount: 0,
        };
        setCollections((prev) => [...prev, newCollection]);
        setIsCreateDialogOpen(false);
        setNewCollectionName("");
        setNewCollectionDescription("");
    };

    const handleEditClick = (collection: Collection) => {
        setEditingCollection(collection);
        setEditName(collection.name);
        setEditDescription(collection.description || "");
        setIsEditDialogOpen(true);
    };

    const handleEditSave = () => {
        if (!editingCollection || !editName.trim()) return;

        setCollections((prev) =>
            prev.map((c) =>
                c.id === editingCollection.id
                    ? {
                        ...c,
                        name: editName.trim(),
                        description: editDescription.trim() || null,
                    }
                    : c
            )
        );
        setIsEditDialogOpen(false);
        setEditingCollection(null);
    };

    const handleDeleteClick = (collection: Collection) => {
        setDeletingCollection(collection);
        setIsDeleteDialogOpen(true);
    };

    const handleDeleteConfirm = () => {
        if (!deletingCollection) return;

        setCollections((prev) =>
            prev.filter((c) => c.id !== deletingCollection.id)
        );
        setIsDeleteDialogOpen(false);
        setDeletingCollection(null);
    };

    return (
        <Container maxWidth={false} sx={{ py: 4 }}>
            <Typography variant="h4" gutterBottom fontWeight={600}>
                Collections
            </Typography>

            {collections.length === 0 ? (
                <EmptyState
                    icon={
                        <FolderIcon
                            sx={{ fontSize: 40, color: "primary.main" }}
                        />
                    }
                    title="No Collections Yet"
                    description="Create your first collection to organize words by topic, book, or course."
                    actionLabel="Create Collection"
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
                    {collections.map((collection) => (
                        <CollectionCard
                            key={collection.id}
                            id={collection.id}
                            name={collection.name}
                            description={collection.description ?? undefined}
                            vocabularyCount={collection.vocabularyCount}
                            onClick={() =>
                                void navigate({
                                    to: "/collections/$collectionId",
                                    params: {
                                        collectionId: String(collection.id),
                                    },
                                })
                            }
                            onEdit={() => handleEditClick(collection)}
                            onDelete={() => handleDeleteClick(collection)}
                        />
                    ))}
                </Box>
            )}

            <Fab
                color="secondary"
                aria-label="Create collection"
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
                <DialogTitle>Create Collection</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        label="Name"
                        value={newCollectionName}
                        onChange={(e) => setNewCollectionName(e.target.value)}
                        sx={{ mt: 1, mb: 2 }}
                    />
                    <TextField
                        fullWidth
                        label="Description (optional)"
                        value={newCollectionDescription}
                        onChange={(e) =>
                            setNewCollectionDescription(e.target.value)
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
                        onClick={handleCreateCollection}
                        disabled={!newCollectionName.trim()}
                    >
                        Create
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={isEditDialogOpen}
                onClose={() => setIsEditDialogOpen(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>Edit Collection</DialogTitle>
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
                <DialogTitle>Delete Collection</DialogTitle>
                <DialogContent>
                    <Typography>
                        Are you sure you want to delete &quot;
                        {deletingCollection?.name}
                        &quot;? This will also delete all vocabularies and
                        entries within it.
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

export const Route = createFileRoute("/_authenticated/collections/")({
    component: CollectionsPage,
});
