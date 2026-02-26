import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import { draftsEntryDetailPath, draftsCreatePath } from "../routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";

import { useDraftsQuery } from "../hooks/useDraftsQuery";
import { DraftsContent } from "../components/DraftsContent";
import { DraftsEmptyState } from "../components/DraftsEmptyState";

import styles from "./DraftsPage.module.scss";

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

    const renderContent = useCallback(() => {
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
            return <DraftsEmptyState onAddDraftClick={handleAddDraftClick} />;
        }

        return (
            <DraftsContent
                entries={data.entries}
                onEntryClick={handleEntryClick}
                onAddDraftClick={handleAddDraftClick}
            />
        );
    }, [
        isLoading,
        isError,
        data,
        refetch,
        handleEntryClick,
        handleAddDraftClick,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav items={[{ label: "Drafts" }]} />
            <PageHeader
                title="Drafts"
                description="Words saved to your default vocabulary"
                actions={
                    <div className={styles.actions}>
                        <Button
                            variant="contained"
                            startIcon={<AddIcon />}
                            onClick={handleAddDraftClick}
                            disabled={isLoading}
                        >
                            Add Draft
                        </Button>
                    </div>
                }
            />
            {renderContent()}
        </PageContainer>
    );
};
