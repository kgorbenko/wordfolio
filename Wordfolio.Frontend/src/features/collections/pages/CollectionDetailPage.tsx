import { useCallback } from "react";
import { useNavigate, useParams } from "@tanstack/react-router";
import { IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

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
import { useVocabulariesQuery } from "../hooks/useVocabulariesQuery";
import { useDeleteCollectionMutation } from "../hooks/useDeleteCollectionMutation";
import { useConfirmDialog } from "../../../contexts/ConfirmDialogContext";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { assertNonNullable } from "../../../utils/misc";

import styles from "./CollectionDetailPage.module.scss";

export const CollectionDetailPage = () => {
    const { collectionId } = useParams({
        from: "/_authenticated/collections/$collectionId/",
    });
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
    } = useVocabulariesQuery(numericId);

    const isLoading = isCollectionLoading || isVocabulariesLoading;
    const isError = isCollectionError || isVocabulariesError;

    const deleteMutation = useDeleteCollectionMutation({
        onSuccess: () => void navigate({ to: "/collections" }),
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
        void navigate({
            to: "/collections/$collectionId/edit",
            params: { collectionId },
        });
    };

    const handleVocabularyClick = (vocabId: number) => {
        void navigate({
            to: "/collections/$collectionId/$vocabularyId",
            params: { collectionId, vocabularyId: String(vocabId) },
        });
    };

    const breadcrumbs: BreadcrumbItem[] = [
        { label: "Collections", to: "/collections" },
        isLoading
            ? { label: "Loading..." }
            : { label: collection?.name ?? "Collection" },
    ];

    const renderContent = useCallback(() => {
        if (isLoading) {
            return <ContentSkeleton variant="detail" />;
        }

        if (isError || !collection || !vocabularies) {
            return (
                <RetryOnError
                    title="Failed to Load Collection"
                    description="Something went wrong while loading this collection."
                    onRetry={() => {
                        void refetchCollection();
                        void refetchVocabularies();
                    }}
                />
            );
        }

        return (
            <CollectionDetailContent
                vocabularies={vocabularies}
                onVocabularyClick={handleVocabularyClick}
                onCreateVocabularyClick={() =>
                    void navigate({
                        to: "/collections/$collectionId/vocabularies/new",
                        params: { collectionId },
                    })
                }
            />
        );
    }, [
        isLoading,
        isError,
        collection,
        vocabularies,
        refetchCollection,
        refetchVocabularies,
        handleVocabularyClick,
        navigate,
        collectionId,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav items={breadcrumbs} />
            <PageHeader
                title={
                    isLoading
                        ? "Loading..."
                        : (collection?.name ?? "Collection")
                }
                description={collection?.description ?? undefined}
                actions={
                    <div className={styles.actions}>
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
