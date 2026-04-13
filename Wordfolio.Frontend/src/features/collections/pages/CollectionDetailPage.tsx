import { useMemo, useCallback } from "react";
import type { GridSortModel } from "@mui/x-data-grid";

import {
    collectionDetailRouteApi,
    collectionsPath,
    collectionEditPath,
} from "../routes";
import type { VocabularySortField } from "../schemas/collectionSchemas";
import {
    vocabularyDetailPath,
    vocabularyCreatePath,
} from "../../vocabularies/routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { PageHeaderActions } from "../../../shared/components/PageHeaderActions";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import type { BreadcrumbItem } from "../../../shared/components/layouts/BreadcrumbNav";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { CollectionDetailContent } from "../components/CollectionDetailContent";
import {
    useCollectionQuery,
    useCollectionVocabulariesQuery,
} from "../../../shared/api/queries/collections";
import { useDeleteCollectionMutation } from "../../../shared/api/mutations/collections";
import { useConfirmDialog } from "../../../shared/contexts/ConfirmDialogContext";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { assertNonNullable } from "../../../shared/utils/misc";

export const CollectionDetailPage = () => {
    const { collectionId } = collectionDetailRouteApi.useParams();
    const { sortField, sortDirection, filter } =
        collectionDetailRouteApi.useSearch();
    const navigate = collectionDetailRouteApi.useNavigate();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { openErrorNotification } = useNotificationContext();

    const sortModel = useMemo<GridSortModel>(() => {
        if (sortField && sortDirection) {
            return [{ field: sortField, sort: sortDirection }];
        }
        return [{ field: "updatedAt", sort: "desc" }];
    }, [sortField, sortDirection]);

    const handleSortModelChange = useCallback(
        (model: GridSortModel) => {
            const first = model[0];
            const isDefault =
                first?.field === "updatedAt" && first?.sort === "desc";
            void navigate({
                to: ".",
                search: (prev) => ({
                    ...prev,
                    sortField: isDefault
                        ? undefined
                        : (first?.field as VocabularySortField | undefined),
                    sortDirection: isDefault
                        ? undefined
                        : (first?.sort ?? undefined),
                }),
                replace: true,
            });
        },
        [navigate]
    );

    const handleFilterValueChange = useCallback(
        (value: string) => {
            void navigate({
                to: ".",
                search: (prev) => ({
                    ...prev,
                    filter: value || undefined,
                }),
                replace: true,
            });
        },
        [navigate]
    );

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
    } = useCollectionVocabulariesQuery(collectionId);

    const isLoading = isCollectionLoading || isVocabulariesLoading;
    const isError = isCollectionError || isVocabulariesError;

    const deleteMutation = useDeleteCollectionMutation({
        onSuccess: async () => {
            await navigate({ ...collectionsPath(), replace: true });
        },
        onError: () =>
            openErrorNotification({ message: "Failed to delete collection" }),
    });

    const handleDelete = async () => {
        assertNonNullable(collection?.id);
        const confirmed = await raiseConfirmDialogAsync({
            title: "Delete Collection",
            message: `Are you sure you want to delete "${collection.name}"? This will also delete all vocabularies and entries within it.`,
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
        (vocabId: string) => {
            void navigate(vocabularyDetailPath(collectionId, vocabId));
        },
        [navigate, collectionId]
    );

    const handleCreateVocabulary = useCallback(() => {
        void navigate(vocabularyCreatePath(collectionId));
    }, [navigate, collectionId]);

    const breadcrumbs: BreadcrumbItem[] = [
        { label: "Collections", ...collectionsPath() },
        isCollectionLoading
            ? { label: "Loading..." }
            : { label: collection?.name ?? "Collection" },
    ];

    const renderContent = () => {
        if (isLoading) {
            return <ContentSkeleton variant="list" />;
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
                sortModel={sortModel}
                onSortModelChange={handleSortModelChange}
                filterValue={filter ?? ""}
                onFilterValueChange={handleFilterValueChange}
            />
        );
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs items={breadcrumbs} />
            <PageHeader
                title={
                    isCollectionLoading
                        ? "Loading..."
                        : (collection?.name ?? "Collection")
                }
                description={collection?.description ?? undefined}
                actions={
                    <PageHeaderActions
                        actions={[
                            {
                                label: "Edit",
                                onClick: handleEditClick,
                                disabled: isLoading,
                            },
                            {
                                label: "Delete",
                                onClick: handleDelete,
                                color: "error",
                                disabled: isLoading || deleteMutation.isPending,
                            },
                        ]}
                    />
                }
            />
            {renderContent()}
        </PageContainer>
    );
};
