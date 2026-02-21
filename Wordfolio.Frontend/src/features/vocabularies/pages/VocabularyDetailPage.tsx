import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { IconButton, Button } from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";

import { vocabularyDetailRouteApi } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import { collectionDetailPath } from "../../../routes/_authenticated/collections/routes";
import { vocabularyEditPath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import { entryDetailPath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/routes";
import { entryCreatePath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/routes";
import { collectionsPath } from "../../../routes/_authenticated/collections/routes";

import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useConfirmDialog } from "../../../contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../utils/misc";

import { useVocabularyQuery } from "../hooks/useVocabularyQuery";
import { useDeleteVocabularyMutation } from "../hooks/useDeleteVocabularyMutation";
import { VocabularyDetailContent } from "../components/VocabularyDetailContent";
import { useEntriesQuery } from "../../entries/hooks/useEntriesQuery";

import styles from "./VocabularyDetailPage.module.scss";

export const VocabularyDetailPage = () => {
    const { collectionId, vocabularyId } = vocabularyDetailRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyQuery(numericCollectionId, numericVocabularyId);

    const {
        data: entries,
        isLoading: isEntriesLoading,
        isError: isEntriesError,
        refetch: refetchEntries,
    } = useEntriesQuery(numericCollectionId, numericVocabularyId);

    const deleteMutation = useDeleteVocabularyMutation({
        onSuccess: () => {
            void navigate(collectionDetailPath(numericCollectionId));
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete vocabulary. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate(
            vocabularyEditPath(numericCollectionId, numericVocabularyId)
        );
    }, [navigate, numericCollectionId, numericVocabularyId]);

    const handleDeleteClick = useCallback(async () => {
        assertNonNullable(vocabulary);

        const confirmed = await raiseConfirmDialogAsync({
            title: "Delete Vocabulary",
            message: `Are you sure you want to delete "${vocabulary.name}"? This will also delete all entries within it.`,
            confirmLabel: "Delete",
            confirmColor: "error",
        });

        if (confirmed) {
            deleteMutation.mutate({
                collectionId: numericCollectionId,
                vocabularyId: numericVocabularyId,
            });
        }
    }, [
        vocabulary,
        raiseConfirmDialogAsync,
        deleteMutation,
        numericCollectionId,
        numericVocabularyId,
    ]);

    const handleEntryClick = useCallback(
        (entryId: number) => {
            void navigate(
                entryDetailPath(
                    numericCollectionId,
                    numericVocabularyId,
                    entryId
                )
            );
        },
        [navigate, numericCollectionId, numericVocabularyId]
    );

    const handleAddWordClick = useCallback(() => {
        void navigate(
            entryCreatePath(numericCollectionId, numericVocabularyId)
        );
    }, [navigate, numericCollectionId, numericVocabularyId]);

    const isLoading = isVocabularyLoading || isEntriesLoading;
    const isError = isVocabularyError || isEntriesError;

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="list" />;

        if (isError || !vocabulary || !entries) {
            const handleRetry = () => {
                if (isVocabularyError) void refetchVocabulary();
                if (isEntriesError) void refetchEntries();
            };

            return (
                <RetryOnError
                    title="Failed to Load Data"
                    description="Something went wrong while loading the data. Please try again."
                    onRetry={handleRetry}
                />
            );
        }

        return (
            <VocabularyDetailContent
                entries={entries}
                onEntryClick={handleEntryClick}
                onAddWordClick={handleAddWordClick}
            />
        );
    }, [
        isLoading,
        isError,
        vocabulary,
        entries,
        isVocabularyError,
        isEntriesError,
        refetchVocabulary,
        refetchEntries,
        handleEntryClick,
        handleAddWordClick,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: vocabulary?.collectionName ?? "...",
                        ...collectionDetailPath(numericCollectionId),
                    },
                    { label: vocabulary?.name ?? "Vocabulary" },
                ]}
            />
            <PageHeader
                title={
                    isLoading
                        ? "Loading..."
                        : (vocabulary?.name ?? "Vocabulary")
                }
                description={vocabulary?.description ?? undefined}
                actions={
                    <div className={styles.actions}>
                        <Button
                            variant="contained"
                            startIcon={<AddIcon />}
                            onClick={handleAddWordClick}
                            disabled={isLoading}
                        >
                            Create Entry
                        </Button>
                        <IconButton
                            color="primary"
                            onClick={handleEditClick}
                            disabled={isLoading}
                            aria-label="Edit vocabulary"
                        >
                            <EditIcon />
                        </IconButton>
                        <IconButton
                            color="error"
                            onClick={handleDeleteClick}
                            disabled={isLoading || deleteMutation.isPending}
                            aria-label="Delete vocabulary"
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
