import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import {
    Box,
    Container,
    Typography,
    IconButton,
    Paper,
    Chip,
    Divider,
    Skeleton,
    Breadcrumbs,
    alpha,
    useTheme,
} from "@mui/material";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { useNotificationContext } from "../../../../../../../contexts/NotificationContext";
import { useConfirmDialog } from "../../../../../../../contexts/ConfirmDialogContext";
import { useEntryQuery } from "../../../../../../../queries/useEntryQuery";
import { useCollectionQuery } from "../../../../../../../features/collections/hooks/useCollectionQuery";
import { useVocabularyQuery } from "../../../../../../../features/vocabularies/hooks/useVocabularyQuery";
import { useDeleteEntryMutation } from "../../../../../../../mutations/useDeleteEntryMutation";
import { RetryOnError } from "../../../../../../../components/common/RetryOnError";
import { assertNonNullable } from "../../../../../../../utils/misc";

const EntryDetailPage = () => {
    const theme = useTheme();
    const { collectionId, vocabularyId, entryId } = Route.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);
    const numericEntryId = Number(entryId);

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
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useEntryQuery(numericEntryId);

    const deleteMutation = useDeleteEntryMutation({
        onSuccess: () => {
            void navigate({
                to: "/collections/$collectionId/$vocabularyId",
                params: { collectionId, vocabularyId },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete entry. Please try again.",
            });
        },
    });

    const handleDelete = async () => {
        assertNonNullable(entry);

        const confirmed = await raiseConfirmDialogAsync({
            title: "Delete Entry",
            message: `Are you sure you want to delete "${entry.entryText}"? This action cannot be undone.`,
            confirmLabel: "Delete",
            confirmColor: "error",
        });

        if (confirmed) {
            deleteMutation.mutate({
                entryId: entry.id,
                vocabularyId: entry.vocabularyId,
            });
        }
    };

    const isLoading =
        isCollectionLoading || isVocabularyLoading || isEntryLoading;
    const isError = isCollectionError || isVocabularyError || isEntryError;

    const handleRetry = () => {
        if (isCollectionError) void refetchCollection();
        if (isVocabularyError) void refetchVocabulary();
        if (isEntryError) void refetchEntry();
    };

    if (isLoading) {
        return (
            <Container maxWidth={false} sx={{ py: 4, maxWidth: 800 }}>
                <Skeleton
                    variant="text"
                    width={300}
                    height={24}
                    sx={{ mb: 2 }}
                />
                <Box
                    sx={{
                        display: "flex",
                        alignItems: "center",
                        gap: 2,
                        mb: 4,
                    }}
                >
                    <Skeleton variant="circular" width={40} height={40} />
                    <Skeleton variant="text" width={200} height={40} />
                </Box>
                <Skeleton
                    variant="text"
                    width={150}
                    height={32}
                    sx={{ mb: 2 }}
                />
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                    {[1, 2].map((i) => (
                        <Skeleton key={i} variant="rounded" height={100} />
                    ))}
                </Box>
            </Container>
        );
    }

    if (isError) {
        return (
            <Container maxWidth={false} sx={{ py: 4, maxWidth: 800 }}>
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
                        Entry
                    </Typography>
                </Breadcrumbs>
                <RetryOnError
                    title="Failed to Load Entry"
                    description="Something went wrong while loading this entry. Please try again."
                    onRetry={handleRetry}
                />
            </Container>
        );
    }

    assertNonNullable(collection);
    assertNonNullable(vocabulary);
    assertNonNullable(entry);

    return (
        <Container maxWidth={false} sx={{ py: 4, maxWidth: 800 }}>
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
                <Link
                    to="/collections/$collectionId/$vocabularyId"
                    params={{ collectionId, vocabularyId }}
                    style={{ textDecoration: "none", color: "inherit" }}
                >
                    <Typography
                        color="text.secondary"
                        sx={{ "&:hover": { color: "primary.main" } }}
                    >
                        {vocabulary.name}
                    </Typography>
                </Link>
                <Typography color="text.primary" fontWeight={500}>
                    {entry.entryText}
                </Typography>
            </Breadcrumbs>

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
                        onClick={() =>
                            navigate({
                                to: "/collections/$collectionId/$vocabularyId",
                                params: { collectionId, vocabularyId },
                            })
                        }
                    >
                        <ArrowBackIcon />
                    </IconButton>
                    <Typography variant="h4" fontWeight={700}>
                        {entry.entryText}
                    </Typography>
                </Box>
                <Box sx={{ display: "flex", gap: 1 }}>
                    <IconButton
                        onClick={() =>
                            void navigate({
                                to: "/collections/$collectionId/$vocabularyId/entries/$entryId/edit",
                                params: { collectionId, vocabularyId, entryId },
                            })
                        }
                        sx={{
                            color: "primary.main",
                            "&:hover": {
                                bgcolor: alpha(theme.palette.primary.main, 0.1),
                            },
                        }}
                    >
                        <EditIcon />
                    </IconButton>
                    <IconButton
                        onClick={() => void handleDelete()}
                        disabled={deleteMutation.isPending}
                        sx={{
                            color: "error.main",
                            "&:hover": {
                                bgcolor: alpha(theme.palette.error.main, 0.1),
                            },
                        }}
                    >
                        <DeleteIcon />
                    </IconButton>
                </Box>
            </Box>

            {entry.definitions.length > 0 && (
                <Box sx={{ mb: 4 }}>
                    <Typography
                        variant="h6"
                        fontWeight={600}
                        sx={{ color: "primary.main", mb: 2 }}
                    >
                        Definitions
                    </Typography>
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 2,
                        }}
                    >
                        {entry.definitions.map((def, index) => (
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
                                        <Typography
                                            variant="body1"
                                            sx={{ lineHeight: 1.6 }}
                                        >
                                            {def.definitionText}
                                        </Typography>
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
                                                    </Box>
                                                ))}
                                            </Box>
                                        )}
                                    </Box>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                </Box>
            )}

            {entry.translations.length > 0 && (
                <Box>
                    <Typography
                        variant="h6"
                        fontWeight={600}
                        sx={{ color: "secondary.main", mb: 2 }}
                    >
                        Translations
                    </Typography>
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "column",
                            gap: 2,
                        }}
                    >
                        {entry.translations.map((trans, index) => (
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
                                        <Typography
                                            variant="body1"
                                            sx={{ lineHeight: 1.6 }}
                                        >
                                            {trans.translationText}
                                        </Typography>
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
                                                    </Box>
                                                ))}
                                            </Box>
                                        )}
                                    </Box>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                </Box>
            )}

            <Divider sx={{ my: 4 }} />

            <Box sx={{ display: "flex", justifyContent: "center" }}>
                <Typography variant="caption" color="text.secondary">
                    Added {new Date(entry.createdAt).toLocaleDateString()}
                    {entry.updatedAt &&
                        ` · Updated ${new Date(entry.updatedAt).toLocaleDateString()}`}
                </Typography>
            </Box>
        </Container>
    );
};

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/$vocabularyId/entries/$entryId/"
)({
    component: EntryDetailPage,
});
