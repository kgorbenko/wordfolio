import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { Box, Container, Typography, Fab, Skeleton } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import FolderIcon from "@mui/icons-material/Folder";

import { CollectionCard } from "../../../components/collections/CollectionCard";
import { EmptyState } from "../../../components/common/EmptyState";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { useCollectionsHierarchyQuery } from "../../../queries/useCollectionsHierarchyQuery";

const CollectionsPage = () => {
    const navigate = useNavigate();
    const { data, isLoading, isError, refetch } =
        useCollectionsHierarchyQuery();

    if (isLoading) {
        return (
            <Container maxWidth={false} sx={{ py: 4 }}>
                <Skeleton
                    variant="text"
                    width={200}
                    height={40}
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
                <Typography variant="h4" gutterBottom fontWeight={600}>
                    Collections
                </Typography>
                <RetryOnError
                    title="Failed to Load Collections"
                    description="Something went wrong while loading your collections. Please try again."
                    onRetry={() => void refetch()}
                />
            </Container>
        );
    }

    const collections = data?.collections ?? [];

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
                    onAction={() => void navigate({ to: "/collections/new" })}
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
                            vocabularyCount={collection.vocabularies.length}
                            onClick={() =>
                                void navigate({
                                    to: "/collections/$collectionId",
                                    params: {
                                        collectionId: String(collection.id),
                                    },
                                })
                            }
                        />
                    ))}
                </Box>
            )}

            <Fab
                color="secondary"
                aria-label="Create collection"
                onClick={() => void navigate({ to: "/collections/new" })}
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

export const Route = createFileRoute("/_authenticated/collections/")({
    component: CollectionsPage,
});
