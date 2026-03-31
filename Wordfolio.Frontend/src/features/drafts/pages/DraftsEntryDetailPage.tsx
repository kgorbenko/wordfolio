import { useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";

import {
    draftsEntryDetailRouteApi,
    draftsEntryEditPath,
    draftsEntryDetailPath,
    draftsPath,
} from "../routes";
import { entryDetailPath } from "../../entries/routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { PageHeaderActions } from "../../../shared/components/PageHeaderActions";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useConfirmDialog } from "../../../shared/contexts/ConfirmDialogContext";
import { assertNonNullable } from "../../../shared/utils/misc";

import { useDraftEntryQuery } from "../../../shared/api/queries/drafts";
import {
    useDeleteDraftEntryMutation,
    useMoveDraftEntryMutation,
} from "../../../shared/api/mutations/drafts";
import { useMoveEntryDialog } from "../../../shared/hooks/useMoveEntryDialog";
import { EntryDetailContent } from "../../../shared/components/entries/EntryDetailContent";

export const DraftsEntryDetailPage = () => {
    const { entryId } = draftsEntryDetailRouteApi.useParams();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { openErrorNotification } = useNotificationContext();
    const { raiseConfirmDialogAsync } = useConfirmDialog();
    const { raiseMoveEntryDialogAsync, dialogElement } = useMoveEntryDialog();

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useDraftEntryQuery(entryId);

    const deleteMutation = useDeleteDraftEntryMutation({
        onSuccess: async () => {
            await navigate(draftsPath());
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to delete entry. Please try again.",
            });
        },
    });

    const moveMutation = useMoveDraftEntryMutation({
        onError: () => {
            openErrorNotification({
                message: "Failed to move entry. Please try again.",
            });
        },
    });

    const handleEditClick = useCallback(() => {
        void navigate(draftsEntryEditPath(entryId));
    }, [navigate, entryId]);

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

        assertNonNullable(moveSelection.vocabularyId);

        moveMutation.mutate(
            {
                entryId: entry.id,
                targetVocabularyId: moveSelection.vocabularyId,
            },
            {
                onSuccess: async (movedEntry) => {
                    if (moveSelection.isDefault) {
                        await navigate(draftsEntryDetailPath(movedEntry.id));
                    } else {
                        assertNonNullable(moveSelection.collectionId);
                        assertNonNullable(moveSelection.vocabularyId);

                        await navigate(
                            entryDetailPath(
                                moveSelection.collectionId,
                                moveSelection.vocabularyId,
                                movedEntry.id
                            )
                        );
                    }

                    void queryClient.invalidateQueries();
                },
            }
        );
    }, [entry, raiseMoveEntryDialogAsync, moveMutation, navigate, queryClient]);

    const isLoading = isEntryLoading;
    const isError = isEntryError;

    const renderContent = () => {
        if (isLoading) return <ContentSkeleton variant="detail" />;

        if (isError || !entry) {
            const handleRetry = () => {
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
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs
                items={[
                    { label: "Drafts", ...draftsPath() },
                    { label: entry?.entryText ?? "Entry" },
                ]}
            />
            <PageHeader
                title={isLoading ? "Loading..." : (entry?.entryText ?? "Entry")}
                actions={
                    <PageHeaderActions
                        actions={[
                            {
                                label: "Move",
                                onClick: handleMoveClick,
                                disabled: isLoading || moveMutation.isPending,
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
                                disabled: isLoading || deleteMutation.isPending,
                            },
                        ]}
                    />
                }
            />
            {renderContent()}
            {dialogElement}
        </PageContainer>
    );
};
