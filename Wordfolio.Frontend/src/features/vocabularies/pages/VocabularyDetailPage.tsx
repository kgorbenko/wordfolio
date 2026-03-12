import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { IconButton, Button } from "@mui/material";

import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";

import { vocabularyDetailRouteApi, vocabularyEditPath } from "../routes";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { entryDetailPath, entryCreatePath } from "../../entries/routes";

import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useConfirmDialog } from "../../../shared/contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../shared/utils/misc";

import { useVocabularyDetailQuery } from "../../../shared/queries/useVocabularyDetailQuery";
import { useDeleteVocabularyMutation } from "../hooks/useDeleteVocabularyMutation";
import { VocabularyDetailContent } from "../components/VocabularyDetailContent";
import { useVocabularyEntriesQuery } from "../hooks/useVocabularyEntriesQuery";

import styles from "./VocabularyDetailPage.module.scss";

export const VocabularyDetailPage = () => {
    const { collectionId, vocabularyId } = vocabularyDetailRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyDetailQuery(collectionId, vocabularyId);

    const {
        data: entries,
        isLoading: isEntriesLoading,
        isError: isEntriesError,
        refetch: refetchEntries,
    } = useVocabularyEntriesQuery(collectionId, vocabularyId);

    const deleteMutation = useDeleteVocabularyMutation({
        onSuccess: () => {
            void navigate(collectionDetailPath(collectionId));
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete vocabulary. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate(vocabularyEditPath(collectionId, vocabularyId));
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
                collectionId,
                vocabularyId,
            });
        }
    }, [
        vocabulary,
        raiseConfirmDialogAsync,
        deleteMutation,
        collectionId,
        vocabularyId,
    ]);

    const handleEntryClick = useCallback(
        (entryId: number) => {
            void navigate(entryDetailPath(collectionId, vocabularyId, entryId));
        },
        [navigate, collectionId, vocabularyId]
    );

    const handleAddWordClick = useCallback(() => {
        void navigate(entryCreatePath(collectionId, vocabularyId));
    }, [navigate, collectionId, vocabularyId]);

    const isLoading = isVocabularyLoading || isEntriesLoading;
    const isError = isVocabularyError || isEntriesError;

    const renderContent = () => {
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
