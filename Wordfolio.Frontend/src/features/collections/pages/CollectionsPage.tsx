import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { CollectionsContent } from "../components/CollectionsContent";
import { useCollectionsQuery } from "../hooks/useCollectionsQuery";
import { CollectionSortBy, SortDirection } from "../types";
import { collectionDetailPath, collectionCreatePath } from "../routes";

export const CollectionsPage = () => {
    const navigate = useNavigate();

    const collectionsQuery = {
        search: "",
        sortBy: CollectionSortBy.UpdatedAt,
        sortDirection: SortDirection.Desc,
    };

    const {
        data: collections,
        isLoading,
        isError,
        refetch,
    } = useCollectionsQuery(collectionsQuery);

    const handleCollectionClick = useCallback(
        (id: number) => {
            void navigate(collectionDetailPath(id));
        },
        [navigate]
    );

    const handleCreateClick = useCallback(() => {
        void navigate(collectionCreatePath());
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
