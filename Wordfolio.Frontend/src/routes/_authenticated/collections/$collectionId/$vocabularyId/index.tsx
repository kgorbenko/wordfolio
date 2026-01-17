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
import { useDeleteVocabularyMutation } from "../../../../../mutations/useDeleteVocabularyMutation";
import { useConfirmDialog } from "../../../../../contexts/ConfirmDialogContext";
import { useNotificationContext } from "../../../../../contexts/NotificationContext";
import { assertNonNullable } from "../../../../../utils/misc";

interface Entry {
    id: number;
    vocabularyId: number;
    entryText: string;
    firstDefinition: string | null;
    firstTranslation: string | null;
    createdAt: string;
}

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

    const entries = stubEntries.filter(
        (e) => e.vocabularyId === numericVocabularyId
    );

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

    const isLoading = isCollectionLoading || isVocabularyLoading;
    const isError = isCollectionError || isVocabularyError;

    const handleRetry = () => {
        if (isCollectionError) {
            void refetchCollection();
        }
        if (isVocabularyError) {
            void refetchVocabulary();
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
            ) : entries.length === 0 ? (
                <Box sx={{ textAlign: "center", py: 4 }}>
                    <Typography variant="body1" color="text.secondary">
                        No words match your search.
                    </Typography>
                </Box>
            ) : (
                <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
                    {entries.map((entry) => (
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
    "/_authenticated/collections/$collectionId/$vocabularyId/"
)({
    component: VocabularyDetailPage,
});
