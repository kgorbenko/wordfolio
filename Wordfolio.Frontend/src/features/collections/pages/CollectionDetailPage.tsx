import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { IconButton, Button } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";

import {
    collectionDetailRouteApi,
    collectionsPath,
    collectionEditPath,
    collectionVocabularyDetailPath,
    collectionVocabularyCreatePath,
} from "../routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import {
    BreadcrumbNav,
    BreadcrumbItem,
} from "../../../shared/components/BreadcrumbNav";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { CollectionDetailContent } from "../components/CollectionDetailContent";
import { useCollectionQuery } from "../hooks/useCollectionQuery";
import { useCollectionVocabulariesQuery } from "../hooks/useVocabulariesSummaryQuery";
import { useDeleteCollectionMutation } from "../hooks/useDeleteCollectionMutation";
import { useConfirmDialog } from "../../../shared/contexts/ConfirmDialogContext";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { assertNonNullable } from "../../../shared/utils/misc";
import { SortDirection, VocabularySortBy } from "../types";

import styles from "./CollectionDetailPage.module.scss";

export const CollectionDetailPage = () => {
    const { collectionId } = collectionDetailRouteApi.useParams();
    const navigate = useNavigate();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { openErrorNotification } = useNotificationContext();

    const {
        data: collection,
        isLoading: isCollectionLoading,
        isError: isCollectionError,
        refetch: refetchCollection,
    } = useCollectionQuery(collectionId);

    const {
        data: vocabularies,
        isLoading: isVocabulariesLoading,
        isError: isVocabulariesError,
        refetch: refetchVocabularies,
    } = useCollectionVocabulariesQuery({
        collectionId,
        sortBy: VocabularySortBy.UpdatedAt,
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
            deleteMutation.mutate(collectionId);
        }
    };

    const handleEditClick = () => {
        void navigate(collectionEditPath(collectionId));
    };

    const handleVocabularyClick = useCallback(
        (vocabId: number) => {
            void navigate(
                collectionVocabularyDetailPath(collectionId, vocabId)
            );
        },
        [navigate, collectionId]
    );

    const handleCreateVocabulary = useCallback(() => {
        void navigate(collectionVocabularyCreatePath(collectionId));
    }, [navigate, collectionId]);

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
