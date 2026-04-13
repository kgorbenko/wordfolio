import { useMemo, useCallback } from "react";
import type { GridSortModel } from "@mui/x-data-grid";

import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { CollectionsContent } from "../components/CollectionsContent";
import { useCollectionsQuery } from "../../../shared/api/queries/collections";
import {
    collectionListRouteApi,
    collectionDetailPath,
    collectionCreatePath,
} from "../routes";
import type { CollectionSortField } from "../schemas/collectionSchemas";

export const CollectionsPage = () => {
    const navigate = collectionListRouteApi.useNavigate();
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
                to: ".",
                search: (prev) => ({
                    ...prev,
                    sortField: isDefault
                        ? undefined
                        : (first?.field as CollectionSortField | undefined),
                    sortDirection: isDefault
                        ? undefined
                        : (first?.sort ?? undefined),
                }),
                replace: true,
            });
        },
        [navigate]
    );

    const handleFilterValueChange = useCallback(
        (value: string) => {
            void navigate({
                to: ".",
                search: (prev) => ({
                    ...prev,
                    filter: value || undefined,
                }),
                replace: true,
            });
        },
        [navigate]
    );

    const {
        data: collections,
        isLoading,
        isError,
        refetch,
    } = useCollectionsQuery();

    const handleCollectionClick = useCallback(
        (id: string) => {
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
