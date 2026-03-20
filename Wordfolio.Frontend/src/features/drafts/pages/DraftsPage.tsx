import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { draftsEntryDetailPath, draftsCreatePath } from "../routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";

import { useDraftsQuery } from "../hooks/useDraftsQuery";
import { DraftsContent } from "../components/DraftsContent";
import { DraftsEmptyState } from "../components/DraftsEmptyState";

export const DraftsPage = () => {
    const navigate = useNavigate();

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
            return <DraftsEmptyState />;
        }

        return (
            <DraftsContent
                entries={data.entries}
                onEntryClick={handleEntryClick}
                onAddDraftClick={handleAddDraftClick}
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
