import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import {
    Container,
    Typography,
    Breadcrumbs,
    Skeleton,
    Box,
} from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { VocabularyForm } from "../../../../../components/vocabularies/VocabularyForm";
import { RetryOnError } from "../../../../../components/common/RetryOnError";
import { VocabularyFormData } from "../../../../../schemas/vocabularySchemas";
import { useCollectionQuery } from "../../../../../queries/useCollectionQuery";
import { useCreateVocabularyMutation } from "../../../../../mutations/useCreateVocabularyMutation";
import { useNotificationContext } from "../../../../../contexts/NotificationContext";

const CreateVocabularyPage = () => {
    const { collectionId } = Route.useParams();
    const navigate = useNavigate();
    const numericCollectionId = Number(collectionId);
    const { openErrorNotification } = useNotificationContext();

    const {
        data: collection,
        isLoading,
        isError,
        refetch,
    } = useCollectionQuery(numericCollectionId);

    const createMutation = useCreateVocabularyMutation({
        onSuccess: (data) => {
            void navigate({
                to: "/collections/$collectionId/$vocabularyId",
                params: {
                    collectionId,
                    vocabularyId: String(data.id),
                },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create vocabulary. Please try again.",
            });
        },
    });

    const handleSubmit = (data: VocabularyFormData) => {
        createMutation.mutate({
            collectionId: numericCollectionId,
            request: {
                name: data.name,
                description: data.description,
            },
        });
    };

    const handleCancel = () => {
        void navigate({
            to: "/collections/$collectionId",
            params: { collectionId },
        });
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

    if (isError || !collection) {
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
                        New Vocabulary
                    </Typography>
                </Breadcrumbs>
                <RetryOnError
                    title="Failed to Load Collection"
                    description="Something went wrong while loading this collection. Please try again."
                    onRetry={() => void refetch()}
                />
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
                    New Vocabulary
                </Typography>
            </Breadcrumbs>

            <Typography variant="h4" gutterBottom fontWeight={600}>
                Create Vocabulary
            </Typography>

            <VocabularyForm
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Create"
                isLoading={createMutation.isPending}
            />
        </Container>
    );
};

export const Route = createFileRoute(
    "/_authenticated/collections/$collectionId/vocabularies/new"
)({
    component: CreateVocabularyPage,
});
