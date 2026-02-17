import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { IconButton, Button } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";

import { collectionDetailRouteApi } from "../../../routes/_authenticated/collections/routes";
import { collectionsPath } from "../../../routes/_authenticated/collections/routes";
import { collectionEditPath } from "../../../routes/_authenticated/collections/routes";
import { vocabularyDetailPath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import { vocabularyCreatePath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import {
    BreadcrumbNav,
    BreadcrumbItem,
} from "../../../components/common/BreadcrumbNav";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { CollectionDetailContent } from "../components/CollectionDetailContent";
import { useCollectionQuery } from "../hooks/useCollectionQuery";
import { useVocabulariesSummaryQuery } from "../hooks/useVocabulariesSummaryQuery";
import { useDeleteCollectionMutation } from "../hooks/useDeleteCollectionMutation";
import { useConfirmDialog } from "../../../contexts/ConfirmDialogContext";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { assertNonNullable } from "../../../utils/misc";
import { SortDirection, VocabularySummarySortBy } from "../api/collectionsApi";

import styles from "./CollectionDetailPage.module.scss";

export const CollectionDetailPage = () => {
    const { collectionId } = collectionDetailRouteApi.useParams();
    const navigate = useNavigate();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { openErrorNotification } = useNotificationContext();

    const numericId = Number(collectionId);

    const {
        data: collection,
        isLoading: isCollectionLoading,
        isError: isCollectionError,
        refetch: refetchCollection,
    } = useCollectionQuery(numericId);

    const {
        data: vocabularies,
        isLoading: isVocabulariesLoading,
        isError: isVocabulariesError,
        refetch: refetchVocabularies,
    } = useVocabulariesSummaryQuery({
        collectionId: numericId,
        sortBy: VocabularySummarySortBy.UpdatedAt,
        sortDirection: SortDirection.Desc,
    });

    const isLoading = isCollectionLoading || isVocabulariesLoading;
    const isError = isCollectionError || isVocabulariesError;

    const deleteMutation = useDeleteCollectionMutation({
        onSuccess: () => void navigate(collectionsPath()),
        onError: () =>
            openErrorNotification({ message: "Failed to delete collection" }),
    });

    const handleDelete = async () => {
        assertNonNullable(collection?.id);
        const confirmed = await raiseConfirmDialogAsync({
            title: "Delete Collection",
            message: `Are you sure you want to delete "${collection.name}"?`,
            confirmLabel: "Delete",
            confirmColor: "error",
        });
        if (confirmed) {
            deleteMutation.mutate(numericId);
        }
    };

    const handleEditClick = () => {
        void navigate(collectionEditPath(numericId));
    };

    const handleVocabularyClick = useCallback(
        (vocabId: number) => {
            void navigate(vocabularyDetailPath(numericId, vocabId));
        },
        [navigate, numericId]
    );

    const handleCreateVocabulary = useCallback(() => {
        void navigate(vocabularyCreatePath(numericId));
    }, [navigate, numericId]);

    const breadcrumbs: BreadcrumbItem[] = [
        { label: "Collections", ...collectionsPath() },
        isCollectionLoading
            ? { label: "Loading..." }
            : { label: collection?.name ?? "Collection" },
    ];

    const renderContent = useCallback(() => {
        if (isLoading) {
            return <ContentSkeleton variant="detail" />;
        }

        if (isError || !collection) {
            return (
                <RetryOnError
                    title="Failed to Load Collection"
                    description="Something went wrong while loading this collection."
                    onRetry={() => {
                        if (isCollectionError) void refetchCollection();
                        if (isVocabulariesError) void refetchVocabularies();
                    }}
                />
            );
        }

        return (
            <CollectionDetailContent
                vocabularies={vocabularies ?? []}
                onVocabularyClick={handleVocabularyClick}
                onCreateVocabularyClick={handleCreateVocabulary}
            />
        );
    }, [
        isLoading,
        isError,
        isCollectionError,
        isVocabulariesError,
        collection,
        vocabularies,
        refetchCollection,
        refetchVocabularies,
        handleVocabularyClick,
        handleCreateVocabulary,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav items={breadcrumbs} />
            <PageHeader
                title={
                    isCollectionLoading
                        ? "Loading..."
                        : (collection?.name ?? "Collection")
                }
                description={collection?.description ?? undefined}
                actions={
                    <div className={styles.actions}>
                        <Button
                            variant="contained"
                            startIcon={<AddIcon />}
                            onClick={handleCreateVocabulary}
                            disabled={isLoading}
                        >
                            Create Vocabulary
                        </Button>
                        <IconButton
                            onClick={handleEditClick}
                            disabled={isLoading}
                            color="primary"
                        >
                            <EditIcon />
                        </IconButton>
                        <IconButton
                            onClick={handleDelete}
                            disabled={isLoading || deleteMutation.isPending}
                            color="error"
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
