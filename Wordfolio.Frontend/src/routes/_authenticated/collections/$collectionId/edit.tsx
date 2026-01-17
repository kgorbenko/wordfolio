import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import {
    Box,
    Container,
    Typography,
    Breadcrumbs,
    Skeleton,
} from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { CollectionForm } from "../../../../components/collections/CollectionForm";
import { RetryOnError } from "../../../../components/common/RetryOnError";
import { CollectionFormData } from "../../../../schemas/collectionSchemas";
import { useCollectionQuery } from "../../../../queries/useCollectionQuery";
import { useUpdateCollectionMutation } from "../../../../mutations/useUpdateCollectionMutation";
import { useNotificationContext } from "../../../../contexts/NotificationContext";

const EditCollectionPage = () => {
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

    const updateMutation = useUpdateCollectionMutation({
        onSuccess: () => {
            void navigate({
                to: "/collections/$collectionId",
                params: { collectionId },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to update collection. Please try again.",
            });
        },
    });

    const handleSubmit = (data: CollectionFormData) => {
        updateMutation.mutate({
            id: numericCollectionId,
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
                        Edit Collection
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
                    Edit
                </Typography>
            </Breadcrumbs>

            <Typography variant="h4" gutterBottom fontWeight={600}>
                Edit Collection
            </Typography>

            <CollectionForm
                defaultValues={{
                    name: collection.name,
                    description: collection.description ?? undefined,
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
    "/_authenticated/collections/$collectionId/edit"
)({
    component: EditCollectionPage,
});
