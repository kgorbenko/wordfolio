import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { Container, Typography, Breadcrumbs } from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

import { CollectionForm } from "../../../components/collections/CollectionForm";
import { CollectionFormData } from "../../../schemas/collectionSchemas";
import { useCreateCollectionMutation } from "../../../mutations/useCreateCollectionMutation";
import { useNotificationContext } from "../../../contexts/NotificationContext";

const CreateCollectionPage = () => {
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();

    const createMutation = useCreateCollectionMutation({
        onSuccess: (data) => {
            void navigate({
                to: "/collections/$collectionId",
                params: { collectionId: String(data.id) },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create collection. Please try again.",
            });
        },
    });

    const handleSubmit = (data: CollectionFormData) => {
        createMutation.mutate({
            name: data.name,
            description: data.description,
        });
    };

    const handleCancel = () => {
        void navigate({ to: "/collections" });
    };

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
                    New Collection
                </Typography>
            </Breadcrumbs>

            <Typography variant="h4" gutterBottom fontWeight={600}>
                Create Collection
            </Typography>

            <CollectionForm
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Create"
                isLoading={createMutation.isPending}
            />
        </Container>
    );
};

export const Route = createFileRoute("/_authenticated/collections/new")({
    component: CreateCollectionPage,
});
