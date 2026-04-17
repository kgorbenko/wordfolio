import { useMemo, useCallback } from "react";
import type { GridSortModel } from "@mui/x-data-grid";

import {
    vocabularyDetailPath,
    vocabularyDetailRouteApi,
    vocabularyEditPath,
    vocabularyPracticePath,
} from "../routes";
import type { EntrySortField } from "../schemas/vocabularySchemas";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { entryDetailPath, entryCreatePath } from "../../entries/routes";

import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { PageHeaderActions } from "../../../shared/components/PageHeaderActions";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useConfirmDialog } from "../../../shared/contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../shared/utils/misc";

import { useVocabularyDetailQuery } from "../../../shared/api/queries/vocabularies";
import {
    useDeleteVocabularyMutation,
    useMoveVocabularyMutation,
} from "../../../shared/api/mutations/vocabularies";
import { useMoveVocabularyDialog } from "../hooks/useMoveVocabularyDialog";
import { VocabularyDetailContent } from "../components/VocabularyDetailContent";
import { useVocabularyEntriesQuery } from "../../../shared/api/queries/entries";

export const VocabularyDetailPage = () => {
    const { collectionId, vocabularyId } = vocabularyDetailRouteApi.useParams();
    const { sortField, sortDirection, filter } =
        vocabularyDetailRouteApi.useSearch();
    const navigate = vocabularyDetailRouteApi.useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();

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
                        : (first?.field as EntrySortField | undefined),
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
        onSuccess: async () => {
            await navigate({
                ...collectionDetailPath(collectionId),
                replace: true,
            });
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete vocabulary. Please try again.",
            });
        },
    });

    const moveMutation = useMoveVocabularyMutation({
        onSuccess: (movedVocabulary) =>
            navigate({
                ...vocabularyDetailPath(
                    movedVocabulary.collectionId,
                    movedVocabulary.id
                ),
                replace: true,
            }),
        onError: () => {
            openErrorNotification({
                message: "Failed to move vocabulary. Please try again.",
            });
        },
    });

    const { raiseMoveVocabularyDialogAsync, dialogElement: moveDialogElement } =
        useMoveVocabularyDialog();

    const handleEditClick = useCallback(() => {
        void navigate(vocabularyEditPath(collectionId, vocabularyId));
    }, [navigate, collectionId, vocabularyId]);

    const handlePracticeClick = useCallback(() => {
        void navigate(vocabularyPracticePath(collectionId, vocabularyId));
    }, [navigate, collectionId, vocabularyId]);

    const handleMoveClick = useCallback(async () => {
        const selection = await raiseMoveVocabularyDialogAsync({
            currentCollectionId: collectionId,
        });

        if (selection) {
            moveMutation.mutate({
                sourceCollectionId: collectionId,
                vocabularyId,
                targetCollectionId: selection.collectionId,
            });
        }
    }, [
        raiseMoveVocabularyDialogAsync,
        moveMutation,
        collectionId,
        vocabularyId,
    ]);

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
        (entryId: string) => {
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
                    title="Failed to Load Vocabulary"
                    description="Something went wrong while loading this vocabulary. Please try again."
                    onRetry={handleRetry}
                />
            );
        }

        return (
            <VocabularyDetailContent
                entries={entries}
                onEntryClick={handleEntryClick}
                onAddWordClick={handleAddWordClick}
                sortModel={sortModel}
                onSortModelChange={handleSortModelChange}
                filterValue={filter ?? ""}
                onFilterValueChange={handleFilterValueChange}
            />
        );
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs
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
                    isError ? undefined : (
                        <PageHeaderActions
                            actions={[
                                {
                                    label: "Practice",
                                    onClick: handlePracticeClick,
                                    disabled: isLoading || !entries?.length,
                                },
                                {
                                    label: "Move",
                                    onClick: handleMoveClick,
                                    disabled:
                                        isLoading || moveMutation.isPending,
                                },
                                {
                                    label: "Edit",
                                    onClick: handleEditClick,
                                    disabled: isLoading,
                                },
                                {
                                    label: "Delete",
                                    onClick: handleDeleteClick,
                                    color: "error",
                                    disabled:
                                        isLoading || deleteMutation.isPending,
                                },
                            ]}
                        />
                    )
                }
            />
            {renderContent()}
            {moveDialogElement}
        </PageContainer>
    );
};
