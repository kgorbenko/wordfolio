import { useMemo, useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import type { GridSortModel } from "@mui/x-data-grid";

import {
    draftsEntryDetailPath,
    draftsCreatePath,
    draftsListRouteApi,
} from "../routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";

import { useDraftsQuery } from "../hooks/useDraftsQuery";
import { EmptyState } from "../../../shared/components/EmptyState";
import { DraftsContent } from "../components/DraftsContent";

export const DraftsPage = () => {
    const navigate = useNavigate();
    const { sortField, sortDirection, filter } = draftsListRouteApi.useSearch();

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
                to: "/drafts",
                search: {
                    sortField: isDefault
                        ? undefined
                        : (first?.field as
                              | "entryText"
                              | "createdAt"
                              | "updatedAt"
                              | "translationCount"
                              | "definitionCount"
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
                to: "/drafts",
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

    const { data, isLoading, isError, refetch } = useDraftsQuery();

    const handleEntryClick = useCallback(
        (entryId: number) => {
            void navigate(draftsEntryDetailPath(entryId));
        },
        [navigate]
    );

    const handleAddDraftClick = useCallback(() => {
        void navigate(draftsCreatePath());
    }, [navigate]);

    const renderContent = () => {
        if (isLoading) return <ContentSkeleton variant="list" />;

        if (isError) {
            return (
                <RetryOnError
                    title="Failed to Load Drafts"
                    description="Something went wrong while loading your drafts. Please try again."
                    onRetry={() => void refetch()}
                />
            );
        }

        if (!data) {
            return <EmptyState />;
        }

        return (
            <DraftsContent
                entries={data.entries}
                onEntryClick={handleEntryClick}
                onAddDraftClick={handleAddDraftClick}
                sortModel={sortModel}
                onSortModelChange={handleSortModelChange}
                filterValue={filter ?? ""}
                onFilterValueChange={handleFilterValueChange}
            />
        );
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs items={[{ label: "Drafts" }]} />
            <PageHeader
                title="Drafts"
                description="Words saved to your default vocabulary"
            />
            {renderContent()}
        </PageContainer>
    );
};
