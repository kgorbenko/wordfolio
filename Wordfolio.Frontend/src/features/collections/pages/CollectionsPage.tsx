import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { CollectionsContent } from "../components/CollectionsContent";
import { useCollectionsQuery } from "../hooks/useCollectionsQuery";

export const CollectionsPage = () => {
    const navigate = useNavigate();

    const {
        data: collections,
        isLoading,
        isError,
        refetch,
    } = useCollectionsQuery();

    const handleCollectionClick = useCallback(
        (id: number) => {
            void navigate({
                to: "/collections/$collectionId",
                params: { collectionId: String(id) },
            });
        },
        [navigate]
    );

    const handleCreateClick = useCallback(() => {
        void navigate({ to: "/collections/new" });
    }, [navigate]);

    const renderContent = useCallback(() => {
        if (isLoading) {
            return <ContentSkeleton variant="cards" />;
        }

        if (isError || !collections) {
            return (
                <RetryOnError
                    title="Failed to Load Collections"
                    description="Something went wrong while loading your collections."
                    onRetry={() => void refetch()}
                />
            );
        }

        return (
            <CollectionsContent
                collections={collections}
                onCollectionClick={handleCollectionClick}
                onCreateClick={handleCreateClick}
            />
        );
    }, [
        isLoading,
        isError,
        collections,
        refetch,
        handleCollectionClick,
        handleCreateClick,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav items={[{ label: "Collections" }]} />
            <PageHeader
                title="Collections"
                actions={
                    <Button
                        variant="contained"
                        startIcon={<AddIcon />}
                        onClick={handleCreateClick}
                    >
                        Create Collection
                    </Button>
                }
            />

            {renderContent()}
        </PageContainer>
    );
};
