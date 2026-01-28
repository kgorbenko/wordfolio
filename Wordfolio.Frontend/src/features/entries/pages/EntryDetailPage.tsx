import { useCallback } from "react";
import { useNavigate, useParams } from "@tanstack/react-router";
import { IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

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
import { EntryDetailContent } from "../components/EntryDetailContent";

import styles from "./EntryDetailPage.module.scss";

export const EntryDetailPage = () => {
    const { collectionId, vocabularyId, entryId } = useParams({
        strict: false,
    });
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();

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
            void navigate({
                to: "/collections/$collectionId/$vocabularyId",
                params: {
                    collectionId: String(collectionId),
                    vocabularyId: String(vocabularyId),
                },
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete entry. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate({
            to: "/collections/$collectionId/$vocabularyId/entries/$entryId/edit",
            params: {
                collectionId: String(collectionId),
                vocabularyId: String(vocabularyId),
                entryId: String(entryId),
            },
        });
    }, [navigate, collectionId, vocabularyId, entryId]);

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
                    { label: "Collections", to: "/collections" },
                    {
                        label: collection?.name ?? "...",
                        to: "/collections/$collectionId",
                        params: { collectionId: String(collectionId) },
                    },
                    {
                        label: vocabulary?.name ?? "...",
                        to: "/collections/$collectionId/$vocabularyId",
                        params: {
                            collectionId: String(collectionId),
                            vocabularyId: String(vocabularyId),
                        },
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
