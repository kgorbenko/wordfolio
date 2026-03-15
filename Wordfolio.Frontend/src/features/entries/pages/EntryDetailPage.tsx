import { useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import DriveFileMoveIcon from "@mui/icons-material/DriveFileMove";

import { entryDetailRouteApi, entryEditPath, entryDetailPath } from "../routes";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { vocabularyDetailPath } from "../../vocabularies/routes";
import { draftsEntryDetailPath } from "../../drafts/routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useConfirmDialog } from "../../../shared/contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../shared/utils/misc";

import { useVocabularyDetailQuery } from "../../../shared/queries/useVocabularyDetailQuery";
import { useEntryQuery } from "../hooks/useEntryQuery";
import { useDeleteEntryMutation } from "../hooks/useDeleteEntryMutation";
import { useMoveEntryMutation } from "../hooks/useMoveEntryMutation";
import { useMoveEntryDialog } from "../../../shared/hooks/useMoveEntryDialog";
import { EntryDetailContent } from "../../../shared/components/entries/EntryDetailContent";

import styles from "./EntryDetailPage.module.scss";

export const EntryDetailPage = () => {
    const { collectionId, vocabularyId, entryId } =
        entryDetailRouteApi.useParams();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { raiseMoveEntryDialogAsync, dialogElement } = useMoveEntryDialog();

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyDetailQuery(collectionId, vocabularyId);

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useEntryQuery(collectionId, vocabularyId, entryId);

    const deleteMutation = useDeleteEntryMutation({
        onSuccess: () => {
            void navigate(vocabularyDetailPath(collectionId, vocabularyId));
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete entry. Please try again.",
            });
        },
    });

    const moveMutation = useMoveEntryMutation({
        onError: () => {
            openErrorNotification({
                message: "Failed to move entry. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate(entryEditPath(collectionId, vocabularyId, entryId));
    }, [collectionId, entryId, navigate, vocabularyId]);

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
                collectionId,
                vocabularyId: entry.vocabularyId,
                entryId: entry.id,
            });
        }
    }, [collectionId, deleteMutation, entry, raiseConfirmDialogAsync]);

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
                collectionId,
                entryId: entry.id,
                sourceVocabularyId: entry.vocabularyId,
                targetVocabularyId: moveSelection.vocabularyId,
            },
            {
                onSuccess: async (movedEntry) => {
                    if (moveSelection.isDefault) {
                        await navigate(draftsEntryDetailPath(movedEntry.id));
                    } else {
                        assertNonNullable(moveSelection.collectionId);

                        await navigate(
                            entryDetailPath(
                                moveSelection.collectionId,
                                moveSelection.vocabularyId,
                                movedEntry.id
                            )
                        );
                    }

                    void queryClient.invalidateQueries();
                },
            }
        );
    }, [
        entry,
        raiseMoveEntryDialogAsync,
        moveMutation,
        navigate,
        queryClient,
        collectionId,
    ]);

    const isLoading = isVocabularyLoading || isEntryLoading;
    const isError = isVocabularyError || isEntryError;

    const renderContent = () => {
        if (isLoading) return <ContentSkeleton variant="detail" />;

        if (isError || !vocabulary || !entry) {
            const handleRetry = () => {
                if (isVocabularyError) void refetchVocabulary();
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
    };

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: vocabulary?.collectionName ?? "...",
                        ...collectionDetailPath(collectionId),
                    },
                    {
                        label: vocabulary?.name ?? "...",
                        ...vocabularyDetailPath(collectionId, vocabularyId),
                    },
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
