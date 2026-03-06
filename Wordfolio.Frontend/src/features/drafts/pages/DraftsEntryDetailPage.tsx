import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import DriveFileMoveIcon from "@mui/icons-material/DriveFileMove";

import {
    collectionVocabularyEntryDetailPath,
    draftsEntryDetailRouteApi,
    draftsEntryDetailPath,
    draftsEntryEditPath,
    draftsPath,
} from "../routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useConfirmDialog } from "../../../shared/contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../shared/utils/misc";

import { useDraftEntryQuery } from "../hooks/useDraftEntryQuery";
import { useDeleteDraftEntryMutation } from "../hooks/useDeleteDraftEntryMutation";
import { useMoveDraftEntryMutation } from "../hooks/useMoveDraftEntryMutation";
import { useMoveEntryDialog } from "../../../shared/hooks/useMoveEntryDialog";
import { EntryDetailContent } from "../../../shared/components/entries/EntryDetailContent";

import styles from "./DraftsEntryDetailPage.module.scss";

export const DraftsEntryDetailPage = () => {
    const { entryId } = draftsEntryDetailRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { raiseMoveEntryDialogAsync, dialogElement } = useMoveEntryDialog();

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useDraftEntryQuery(entryId);

    const deleteMutation = useDeleteDraftEntryMutation({
        onSuccess: () => {
            void navigate(draftsPath());
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete entry. Please try again.",
            });
        },
    });

    const moveMutation = useMoveDraftEntryMutation({
        onError: () => {
            openErrorNotification({
                message: "Failed to move entry. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate(draftsEntryEditPath(entryId));
    }, [navigate, entryId]);

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
            });
        }
    }, [entry, raiseConfirmDialogAsync, deleteMutation]);

    const handleMoveClick = useCallback(async () => {
        assertNonNullable(entry);

        const moveSelection = await raiseMoveEntryDialogAsync({
            currentVocabularyId: entry.vocabularyId,
        });

        if (!moveSelection) {
            return;
        }

        moveMutation.mutate(
            {
                entryId: entry.id,
                targetVocabularyId: moveSelection.vocabularyId,
            },
            {
                onSuccess: (movedEntry) => {
                    if (moveSelection.isDefault) {
                        void navigate(draftsEntryDetailPath(movedEntry.id));
                        return;
                    }

                    assertNonNullable(moveSelection.collectionId);

                    void navigate(
                        collectionVocabularyEntryDetailPath(
                            moveSelection.collectionId,
                            moveSelection.vocabularyId,
                            movedEntry.id
                        )
                    );
                },
            }
        );
    }, [entry, raiseMoveEntryDialogAsync, moveMutation, navigate]);

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
                            onClick={handleMoveClick}
                            disabled={isLoading || moveMutation.isPending}
                            aria-label="Move entry"
                        >
                            <DriveFileMoveIcon />
                        </IconButton>
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
            {dialogElement}
        </PageContainer>
    );
};
