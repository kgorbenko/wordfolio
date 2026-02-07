import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import DriveFileMoveIcon from "@mui/icons-material/DriveFileMove";

import { entryDetailRouteApi } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/routes";
import { collectionsPath } from "../../../routes/_authenticated/collections/routes";
import { collectionDetailPath } from "../../../routes/_authenticated/collections/routes";
import { vocabularyDetailPath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import {
    entryEditPath,
    entryDetailPath,
} from "../../../routes/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/routes";
import { draftsEntryDetailPath } from "../../../routes/_authenticated/drafts/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useConfirmDialog } from "../../../contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../utils/misc";

import { useCollectionQuery } from "../../collections/hooks/useCollectionQuery";
import { useVocabularyQuery } from "../../vocabularies/hooks/useVocabularyQuery";
import { useEntryQuery } from "../hooks/useEntryQuery";
import { useDeleteEntryMutation } from "../hooks/useDeleteEntryMutation";
import { useMoveEntryMutation } from "../hooks/useMoveEntryMutation";
import { useMoveEntryDialog } from "../hooks/useMoveEntryDialog";
import { EntryDetailContent } from "../components/EntryDetailContent";

import styles from "./EntryDetailPage.module.scss";

export const EntryDetailPage = () => {
    const { collectionId, vocabularyId, entryId } =
        entryDetailRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { raiseMoveEntryDialogAsync, dialogElement } = useMoveEntryDialog();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);
    const numericEntryId = Number(entryId);

    const {
        data: collection,
        isLoading: isCollectionLoading,
        isError: isCollectionError,
        refetch: refetchCollection,
    } = useCollectionQuery(numericCollectionId);

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyQuery(numericCollectionId, numericVocabularyId);

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useEntryQuery(numericEntryId);

    const deleteMutation = useDeleteEntryMutation({
        onSuccess: () => {
            void navigate(
                vocabularyDetailPath(numericCollectionId, numericVocabularyId)
            );
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
        void navigate(
            entryEditPath(
                numericCollectionId,
                numericVocabularyId,
                numericEntryId
            )
        );
    }, [navigate, numericCollectionId, numericVocabularyId, numericEntryId]);

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
                sourceVocabularyId: entry.vocabularyId,
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
                        entryDetailPath(
                            moveSelection.collectionId,
                            moveSelection.vocabularyId,
                            movedEntry.id
                        )
                    );
                },
            }
        );
    }, [entry, raiseMoveEntryDialogAsync, moveMutation, navigate]);

    const isLoading =
        isCollectionLoading || isVocabularyLoading || isEntryLoading;
    const isError = isCollectionError || isVocabularyError || isEntryError;

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="detail" />;

        if (isError || !collection || !vocabulary || !entry) {
            const handleRetry = () => {
                if (isCollectionError) void refetchCollection();
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
    }, [
        isLoading,
        isError,
        collection,
        vocabulary,
        entry,
        isCollectionError,
        isVocabularyError,
        isEntryError,
        refetchCollection,
        refetchVocabulary,
        refetchEntry,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: collection?.name ?? "...",
                        ...collectionDetailPath(numericCollectionId),
                    },
                    {
                        label: vocabulary?.name ?? "...",
                        ...vocabularyDetailPath(
                            numericCollectionId,
                            numericVocabularyId
                        ),
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
