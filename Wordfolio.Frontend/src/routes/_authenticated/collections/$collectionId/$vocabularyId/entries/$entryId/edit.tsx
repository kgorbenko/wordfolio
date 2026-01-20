import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import {
    Box,
    Container,
    Typography,
    Breadcrumbs,
    Skeleton,
} from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import {
    EntryForm,
    EntryFormValues,
    EntryFormOutput,
} from "../../../../../../../features/entries/components/EntryForm";
import { RetryOnError } from "../../../../../../../components/common/RetryOnError";
import { useEntryQuery } from "../../../../../../../queries/useEntryQuery";
import { useCollectionQuery } from "../../../../../../../features/collections/hooks/useCollectionQuery";
import { useVocabularyQuery } from "../../../../../../../features/vocabularies/hooks/useVocabularyQuery";
import { useUpdateEntryMutation } from "../../../../../../../mutations/useUpdateEntryMutation";
import { useNotificationContext } from "../../../../../../../contexts/NotificationContext";
import { assertNonNullable } from "../../../../../../../utils/misc";
import { EntryResponse } from "../../../../../../../api/entriesApi";

const mapEntryToFormValues = (entry: EntryResponse): EntryFormValues => ({
    entryText: entry.entryText,
    definitions: entry.definitions.map((def) => ({
        id: String(def.id),
        definitionText: def.definitionText,
        source: def.source,
        examples: def.examples.map((ex) => ({
            id: String(ex.id),
            exampleText: ex.exampleText,
            source: ex.source,
        })),
    })),
    translations: entry.translations.map((trans) => ({
        id: String(trans.id),
        translationText: trans.translationText,
        source: trans.source,
        examples: trans.examples.map((ex) => ({
            id: String(ex.id),
            exampleText: ex.exampleText,
            source: ex.source,
        })),
    })),
});

const EditEntryPage = () => {
    const { collectionId, vocabularyId, entryId } = Route.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();

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

    const updateMutation = useUpdateEntryMutation({
        onSuccess: () => {
            void navigate({
                to: "/collections/$collectionId/$vocabularyId/entries/$entryId",
                params: { collectionId, vocabularyId, entryId },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to update entry. Please try again.",
            });
        },
    });

    const handleSubmit = (data: EntryFormOutput) => {
        updateMutation.mutate({
            entryId: numericEntryId,
            request: {
                entryText: data.entryText,
                definitions: data.definitions,
                translations: data.translations,
            },
        });
    };

    const handleCancel = () => {
        void navigate({
            to: "/collections/$collectionId/$vocabularyId/entries/$entryId",
            params: { collectionId, vocabularyId, entryId },
        });
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
                    sx={{ mb: 3 }}
                />
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                    <Skeleton variant="rounded" height={56} />
                    <Skeleton variant="rounded" height={100} />
                    <Skeleton
                        variant="rounded"
                        height={36}
                        width={150}
                        sx={{ alignSelf: "flex-end" }}
                    />
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
                        Edit Entry
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
                <Link
                    to="/collections/$collectionId/$vocabularyId/entries/$entryId"
                    params={{ collectionId, vocabularyId, entryId }}
                    style={{ textDecoration: "none", color: "inherit" }}
                >
                    <Typography
                        color="text.secondary"
                        sx={{ "&:hover": { color: "primary.main" } }}
                    >
                        {entry.entryText}
                    </Typography>
                </Link>
                <Typography color="text.primary" fontWeight={500}>
                    Edit
                </Typography>
            </Breadcrumbs>

            <Typography variant="h4" gutterBottom fontWeight={600}>
                Edit Entry
            </Typography>

            <EntryForm
                defaultValues={mapEntryToFormValues(entry)}
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Save"
                isLoading={updateMutation.isPending}
            />
        </Container>
    );
};

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/$vocabularyId/entries/$entryId/edit"
)({
    component: EditEntryPage,
});
