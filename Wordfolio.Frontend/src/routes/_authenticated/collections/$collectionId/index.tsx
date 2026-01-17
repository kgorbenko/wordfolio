import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import {
    Box,
    Container,
    Typography,
    Fab,
    Breadcrumbs,
    Skeleton,
    IconButton,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

import { VocabularyCard } from "../../../../components/vocabularies/VocabularyCard";
import { EmptyState } from "../../../../components/common/EmptyState";
import { RetryOnError } from "../../../../components/common/RetryOnError";
import { useCollectionQuery } from "../../../../queries/useCollectionQuery";
import { useVocabulariesQuery } from "../../../../queries/useVocabulariesQuery";
import { useDeleteCollectionMutation } from "../../../../mutations/useDeleteCollectionMutation";
import { useConfirmDialog } from "../../../../contexts/ConfirmDialogContext";
import { useNotificationContext } from "../../../../contexts/NotificationContext";
import { assertNonNullable } from "../../../../utils/misc";

const CollectionDetailPage = () => {
    const { collectionId } = Route.useParams();
    const navigate = useNavigate();
    const numericCollectionId = Number(collectionId);
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { openErrorNotification } = useNotificationContext();

    const {
        data: collection,
        isLoading: isCollectionLoading,
        isError: isCollectionError,
        refetch: refetchCollection,
    } = useCollectionQuery(numericCollectionId);

    const {
        data: vocabularies,
        isLoading: isVocabulariesLoading,
        isError: isVocabulariesError,
        refetch: refetchVocabularies,
    } = useVocabulariesQuery(numericCollectionId);

    const deleteMutation = useDeleteCollectionMutation({
        onSuccess: () => {
            void navigate({ to: "/collections" });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete collection. Please try again.",
            });
        },
    });

    const handleDeleteCollection = async () => {
        if (!collection) return;

        const confirmed = await raiseConfirmDialogAsync({
            title: "Delete Collection",
            message: `Are you sure you want to delete "${collection.name}"? This will also delete all vocabularies and entries within it.`,
            confirmLabel: "Delete",
            confirmColor: "error",
        });

        if (confirmed) {
            deleteMutation.mutate(numericCollectionId);
        }
    };

    const isLoading = isCollectionLoading || isVocabulariesLoading;
    const isError = isCollectionError || isVocabulariesError;

    const handleRetry = () => {
        if (isCollectionError) {
            void refetchCollection();
        }
        if (isVocabulariesError) {
            void refetchVocabularies();
        }
    };

    if (isLoading) {
        return (
            <Container maxWidth={false} sx={{ py: 4 }}>
                <Skeleton
                    variant="text"
                    width={300}
                    height={24}
                    sx={{ mb: 2 }}
                />
                <Skeleton
                    variant="text"
                    width={200}
                    height={40}
                    sx={{ mb: 1 }}
                />
                <Skeleton
                    variant="text"
                    width={400}
                    height={24}
                    sx={{ mb: 3 }}
                />
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
                    {[1, 2, 3].map((i) => (
                        <Skeleton key={i} variant="rounded" height={120} />
                    ))}
                </Box>
            </Container>
        );
    }

    if (isError) {
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
                        Collection
                    </Typography>
                </Breadcrumbs>
                <RetryOnError
                    title="Failed to Load Collection"
                    description="Something went wrong while loading this collection. Please try again."
                    onRetry={handleRetry}
                />
            </Container>
        );
    }

    assertNonNullable(collection);
    assertNonNullable(vocabularies);

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

            <Box
                sx={{
                    display: "flex",
                    alignItems: "flex-start",
                    justifyContent: "space-between",
                    mb: collection.description ? 0 : 3,
                }}
            >
                <Typography variant="h4" fontWeight={600}>
                    {collection.name}
                </Typography>
                <Box sx={{ display: "flex", gap: 1 }}>
                    <IconButton
                        aria-label="Edit collection"
                        color="primary"
                        onClick={() =>
                            void navigate({
                                to: "/collections/$collectionId/edit",
                                params: { collectionId },
                            })
                        }
                    >
                        <EditIcon />
                    </IconButton>
                    <IconButton
                        aria-label="Delete collection"
                        color="error"
                        onClick={() => void handleDeleteCollection()}
                        disabled={deleteMutation.isPending}
                    >
                        <DeleteIcon />
                    </IconButton>
                </Box>
            </Box>
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
                    onAction={() =>
                        void navigate({
                            to: "/collections/$collectionId/vocabularies/new",
                            params: { collectionId },
                        })
                    }
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
                            entryCount={0}
                            onClick={() =>
                                void navigate({
                                    to: "/collections/$collectionId/$vocabularyId",
                                    params: {
                                        collectionId: String(collectionId),
                                        vocabularyId: String(vocab.id),
                                    },
                                })
                            }
                        />
                    ))}
                </Box>
            )}

            <Fab
                color="secondary"
                aria-label="Add vocabulary"
                onClick={() =>
                    void navigate({
                        to: "/collections/$collectionId/vocabularies/new",
                        params: { collectionId },
                    })
                }
                sx={{
                    position: "fixed",
                    bottom: { xs: 140, md: 24 },
                    right: { xs: 24, md: 100 },
                }}
            >
                <AddIcon />
            </Fab>
        </Container>
    );
};

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/"
)({
    component: CollectionDetailPage,
});
