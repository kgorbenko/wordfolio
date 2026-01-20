import { useCallback } from "react";
import { useNavigate, useParams } from "@tanstack/react-router";
import { IconButton, Button } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";

import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useConfirmDialog } from "../../../contexts/ConfirmDialogContext";
import { useUiStore } from "../../../stores/uiStore";
import { assertNonNullable } from "../../../utils/misc";

import { useCollectionQuery } from "../../collections/hooks/useCollectionQuery";
import { useVocabularyQuery } from "../hooks/useVocabularyQuery";
import { useDeleteVocabularyMutation } from "../hooks/useDeleteVocabularyMutation";
import { VocabularyDetailContent } from "../components/VocabularyDetailContent";

import { useEntriesQuery } from "../hooks/useEntriesQuery";

import styles from "./VocabularyDetailPage.module.scss";

export const VocabularyDetailPage = () => {
    const { collectionId, vocabularyId } = useParams({ strict: false });
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { openWordEntry } = useUiStore();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);

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

    // Using old hook for entries
    const {
        data: entries,
        isLoading: isEntriesLoading,
        isError: isEntriesError,
        refetch: refetchEntries,
    } = useEntriesQuery(numericVocabularyId);

    const deleteMutation = useDeleteVocabularyMutation({
        onSuccess: () => {
            void navigate({
                to: "/collections/$collectionId",
                params: { collectionId: String(collectionId) },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete vocabulary. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate({
            to: "/collections/$collectionId/$vocabularyId/edit",
            params: {
                collectionId: String(collectionId),
                vocabularyId: String(vocabularyId),
            },
        });
    }, [navigate, collectionId, vocabularyId]);

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
            void navigate({
                to: "/collections/$collectionId/$vocabularyId/entries/$entryId",
                params: {
                    collectionId: String(collectionId),
                    vocabularyId: String(vocabularyId),
                    entryId: String(entryId),
                },
            });
        },
        [navigate, collectionId, vocabularyId]
    );

    const handleAddWordClick = useCallback(() => {
        openWordEntry(numericVocabularyId);
    }, [openWordEntry, numericVocabularyId]);

    const isLoading =
        isCollectionLoading || isVocabularyLoading || isEntriesLoading;
    const isError = isCollectionError || isVocabularyError || isEntriesError;

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="list" />;

        if (isError || !collection || !vocabulary || !entries) {
            const handleRetry = () => {
                if (isCollectionError) void refetchCollection();
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
        collection,
        vocabulary,
        entries,
        isCollectionError,
        isVocabularyError,
        isEntriesError,
        refetchCollection,
        refetchVocabulary,
        refetchEntries,
        handleEntryClick,
        handleAddWordClick,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", to: "/collections" },
                    {
                        label: collection?.name ?? "...",
                        to: "/collections/$collectionId",
                        params: { collectionId: String(collectionId) },
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
