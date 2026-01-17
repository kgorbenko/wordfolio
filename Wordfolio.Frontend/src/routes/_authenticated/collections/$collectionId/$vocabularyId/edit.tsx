import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import {
    Box,
    Container,
    Typography,
    Breadcrumbs,
    Skeleton,
} from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { VocabularyForm } from "../../../../../components/vocabularies/VocabularyForm";
import { RetryOnError } from "../../../../../components/common/RetryOnError";
import { VocabularyFormData } from "../../../../../schemas/vocabularySchemas";
import { useCollectionQuery } from "../../../../../queries/useCollectionQuery";
import { useVocabularyQuery } from "../../../../../queries/useVocabularyQuery";
import { useUpdateVocabularyMutation } from "../../../../../mutations/useUpdateVocabularyMutation";
import { useNotificationContext } from "../../../../../contexts/NotificationContext";
import { assertNonNullable } from "../../../../../utils/misc.ts";

const EditVocabularyPage = () => {
    const { collectionId, vocabularyId } = Route.useParams();
    const navigate = useNavigate();
    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);
    const { openErrorNotification } = useNotificationContext();

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

    const updateMutation = useUpdateVocabularyMutation({
        onSuccess: () => {
            void navigate({
                to: "/collections/$collectionId/$vocabularyId",
                params: { collectionId, vocabularyId },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to update vocabulary. Please try again.",
            });
        },
    });

    const handleSubmit = (data: VocabularyFormData) => {
        updateMutation.mutate({
            collectionId: numericCollectionId,
            vocabularyId: numericVocabularyId,
            request: {
                name: data.name,
                description: data.description,
            },
        });
    };

    const handleCancel = () => {
        void navigate({
            to: "/collections/$collectionId/$vocabularyId",
            params: { collectionId, vocabularyId },
        });
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
                        Edit Vocabulary
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
                    Edit
                </Typography>
            </Breadcrumbs>

            <Typography variant="h4" gutterBottom fontWeight={600}>
                Edit Vocabulary
            </Typography>

            <VocabularyForm
                defaultValues={{
                    name: vocabulary.name,
                    description: vocabulary.description ?? undefined,
                }}
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Save"
                isLoading={updateMutation.isPending}
            />
        </Container>
    );
};

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/$vocabularyId/edit"
)({
    component: EditVocabularyPage,
});
