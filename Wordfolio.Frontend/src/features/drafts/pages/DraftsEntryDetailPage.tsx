import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

import {
    draftsEntryDetailRouteApi,
    draftsPath,
    draftsEntryEditPath,
} from "../../../routes/_authenticated/drafts/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useConfirmDialog } from "../../../contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../utils/misc";

import { useEntryQuery } from "../../entries/hooks/useEntryQuery";
import { useDeleteEntryMutation } from "../../entries/hooks/useDeleteEntryMutation";
import { EntryDetailContent } from "../../entries/components/EntryDetailContent";

import styles from "./DraftsEntryDetailPage.module.scss";

export const DraftsEntryDetailPage = () => {
    const { entryId } = draftsEntryDetailRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();

    const numericEntryId = Number(entryId);

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useEntryQuery(numericEntryId);

    const deleteMutation = useDeleteEntryMutation({
        onSuccess: () => {
            void navigate(draftsPath());
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete entry. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate(draftsEntryEditPath(numericEntryId));
    }, [navigate, numericEntryId]);

    const handleDeleteClick = useCallback(async () => {
        assertNonNullable(entry);

        const confirmed = await raiseConfirmDialogAsync({
            title: "Delete Entry",
            message: `Are you sure you want to delete "${entry.entryText}"? This action cannot be undone.`,
            confirmLabel: "Delete",
            confirmColor: "error",
        });

        if (confirmed) {
            deleteMutation.mutate({
                entryId: entry.id,
                vocabularyId: entry.vocabularyId,
            });
        }
    }, [entry, raiseConfirmDialogAsync, deleteMutation]);

    const isLoading = isEntryLoading;
    const isError = isEntryError;

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="detail" />;

        if (isError || !entry) {
            const handleRetry = () => {
                if (isEntryError) void refetchEntry();
            };

            return (
                <RetryOnError
                    title="Failed to Load Entry"
                    description="Something went wrong while loading this entry. Please try again."
                    onRetry={handleRetry}
                />
            );
        }

        return <EntryDetailContent entry={entry} />;
    }, [isLoading, isError, entry, isEntryError, refetchEntry]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Drafts", ...draftsPath() },
                    { label: entry?.entryText ?? "Entry" },
                ]}
            />
            <PageHeader
                title={isLoading ? "Loading..." : (entry?.entryText ?? "Entry")}
                actions={
                    <div className={styles.actions}>
                        <IconButton
                            color="primary"
                            onClick={handleEditClick}
                            disabled={isLoading}
                            aria-label="Edit entry"
                        >
                            <EditIcon />
                        </IconButton>
                        <IconButton
                            color="error"
                            onClick={handleDeleteClick}
                            disabled={isLoading || deleteMutation.isPending}
                            aria-label="Delete entry"
                        >
                            <DeleteIcon />
                        </IconButton>
                    </div>
                }
            />
            {renderContent()}
        </PageContainer>
    );
};
