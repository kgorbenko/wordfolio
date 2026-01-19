import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import {
    Box,
    Container,
    Typography,
    Breadcrumbs,
    Skeleton,
    IconButton,
} from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

import { EntryListItem } from "../../../../../components/entries/EntryListItem";
import { EmptyState } from "../../../../../components/common/EmptyState";
import { RetryOnError } from "../../../../../components/common/RetryOnError";
import { useUiStore } from "../../../../../stores/uiStore";
import { useCollectionQuery } from "../../../../../queries/useCollectionQuery";
import { useVocabularyQuery } from "../../../../../queries/useVocabularyQuery";
import { useEntriesQuery } from "../../../../../queries/useEntriesQuery";
import { useDeleteVocabularyMutation } from "../../../../../mutations/useDeleteVocabularyMutation";
import { useConfirmDialog } from "../../../../../contexts/ConfirmDialogContext";
import { useNotificationContext } from "../../../../../contexts/NotificationContext";
import { assertNonNullable } from "../../../../../utils/misc";

const VocabularyDetailPage = () => {
    const { collectionId, vocabularyId } = Route.useParams();
    const navigate = useNavigate();
    const { openWordEntry } = useUiStore();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { openErrorNotification } = useNotificationContext();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);

    const {
        data: collection,
        isLoading: isCollectionLoading,
        isError: isCollectionError,
        refetch: refetchCollection,
    } = useCollectionQuery(numericCollectionId);

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyQuery(numericCollectionId, numericVocabularyId);

    const {
        data: entries,
        isLoading: isEntriesLoading,
        isError: isEntriesError,
        refetch: refetchEntries,
    } = useEntriesQuery(numericVocabularyId);

    const deleteMutation = useDeleteVocabularyMutation({
        onSuccess: () => {
            void navigate({
                to: "/collections/$collectionId",
                params: { collectionId },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete vocabulary. Please try again.",
            });
        },
    });

    const handleDeleteVocabulary = async () => {
        assertNonNullable(vocabulary);

        const confirmed = await raiseConfirmDialogAsync({
            title: "Delete Vocabulary",
            message: `Are you sure you want to delete "${vocabulary.name}"? This will also delete all entries within it.`,
            confirmLabel: "Delete",
            confirmColor: "error",
        });

        if (confirmed) {
            deleteMutation.mutate({
                collectionId: numericCollectionId,
                vocabularyId: numericVocabularyId,
            });
        }
    };

    const isLoading =
        isCollectionLoading || isVocabularyLoading || isEntriesLoading;
    const isError = isCollectionError || isVocabularyError || isEntriesError;

    const handleRetry = () => {
        if (isCollectionError) {
            void refetchCollection();
        }
        if (isVocabularyError) {
            void refetchVocabulary();
        }
        if (isEntriesError) {
            void refetchEntries();
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
                <Skeleton variant="rounded" height={56} sx={{ mb: 2 }} />
                <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
                    {[1, 2, 3].map((i) => (
                        <Skeleton key={i} variant="rounded" height={80} />
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
                        Vocabulary
                    </Typography>
                </Breadcrumbs>
                <RetryOnError
                    title="Failed to Load Vocabulary"
                    description="Something went wrong while loading this vocabulary. Please try again."
                    onRetry={handleRetry}
                />
            </Container>
        );
    }

    assertNonNullable(collection);
    assertNonNullable(vocabulary);
    assertNonNullable(entries);

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
                    alignItems: "flex-start",
                    mb: vocabulary.description ? 0 : 3,
                }}
            >
                <Box>
                    <Typography variant="h4" fontWeight={600}>
                        {vocabulary.name}
                    </Typography>
                    {vocabulary.description && (
                        <Typography
                            variant="body1"
                            color="text.secondary"
                            sx={{ mb: 3 }}
                        >
                            {vocabulary.description}
                        </Typography>
                    )}
                </Box>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                    <Typography variant="body2" color="text.secondary">
                        {entries.length}{" "}
                        {entries.length === 1 ? "word" : "words"}
                    </Typography>
                    <IconButton
                        aria-label="Edit vocabulary"
                        color="primary"
                        onClick={() =>
                            void navigate({
                                to: "/collections/$collectionId/$vocabularyId/edit",
                                params: { collectionId, vocabularyId },
                            })
                        }
                    >
                        <EditIcon />
                    </IconButton>
                    <IconButton
                        aria-label="Delete vocabulary"
                        color="error"
                        onClick={() => void handleDeleteVocabulary()}
                        disabled={deleteMutation.isPending}
                    >
                        <DeleteIcon />
                    </IconButton>
                </Box>
            </Box>

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
            ) : (
                <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
                    {entries.map((entry) => (
                        <EntryListItem
                            key={entry.id}
                            id={entry.id}
                            entryText={entry.entryText}
                            firstDefinition={
                                entry.definitions[0]?.definitionText
                            }
                            firstTranslation={
                                entry.translations[0]?.translationText
                            }
                            createdAt={entry.createdAt}
                            onClick={() =>
                                void navigate({
                                    to: "/collections/$collectionId/$vocabularyId/entries/$entryId",
                                    params: {
                                        collectionId,
                                        vocabularyId,
                                        entryId: String(entry.id),
                                    },
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
    "/_authenticated/collections/$collectionId/$vocabularyId/"
)({
    component: VocabularyDetailPage,
});
