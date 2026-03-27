import { useMemo, useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import type { GridSortModel } from "@mui/x-data-grid";

import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { CollectionsContent } from "../components/CollectionsContent";
import { useCollectionsQuery } from "../hooks/useCollectionsQuery";
import {
    collectionListRouteApi,
    collectionDetailPath,
    collectionCreatePath,
} from "../routes";

export const CollectionsPage = () => {
    const navigate = useNavigate();
    const { sortField, sortDirection, filter } =
        collectionListRouteApi.useSearch();

    const sortModel = useMemo<GridSortModel>(() => {
        if (sortField && sortDirection) {
            return [{ field: sortField, sort: sortDirection }];
        }
        return [{ field: "updatedAt", sort: "desc" }];
    }, [sortField, sortDirection]);

    const handleSortModelChange = useCallback(
        (model: GridSortModel) => {
            const first = model[0];
            const isDefault =
                first?.field === "updatedAt" && first?.sort === "desc";
            void navigate({
                to: "/collections",
                search: {
                    sortField: isDefault
                        ? undefined
                        : (first?.field as
                              | "name"
                              | "createdAt"
                              | "updatedAt"
                              | "vocabularyCount"
                              | undefined),
                    sortDirection: isDefault
                        ? undefined
                        : (first?.sort ?? undefined),
                    filter: filter || undefined,
                },
                replace: true,
            });
        },
        [navigate, filter]
    );

    const handleFilterValueChange = useCallback(
        (value: string) => {
            void navigate({
                to: "/collections",
                search: {
                    sortField,
                    sortDirection,
                    filter: value || undefined,
                },
                replace: true,
            });
        },
        [navigate, sortField, sortDirection]
    );

    const {
        data: collections,
        isLoading,
        isError,
        refetch,
    } = useCollectionsQuery();

    const handleCollectionClick = useCallback(
        (id: number) => {
            void navigate(collectionDetailPath(id));
        },
        [navigate]
    );

    const handleCreateClick = useCallback(() => {
        void navigate(collectionCreatePath());
    }, [navigate]);

    const renderContent = () => {
        if (isLoading) {
            return <ContentSkeleton variant="list" />;
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
                sortModel={sortModel}
                onSortModelChange={handleSortModelChange}
                filterValue={filter ?? ""}
                onFilterValueChange={handleFilterValueChange}
            />
        );
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs items={[{ label: "Collections" }]} />
            <PageHeader title="Collections" />
            {renderContent()}
        </PageContainer>
    );
};
