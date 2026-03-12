import { useCallback, useRef } from "react";
import { useNavigate } from "@tanstack/react-router";

import { entryCreateRouteApi } from "../routes";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { vocabularyDetailPath } from "../../vocabularies/routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useVocabularyDetailQuery } from "../../../shared/queries/useVocabularyDetailQuery";
import { useDuplicateEntryDialog } from "../../../shared/hooks/useDuplicateEntryDialog";
import { useCreateEntryMutation } from "../../../shared/queries/useCreateEntryMutation";
import type { CreateEntryData } from "../../../shared/types/entries";
import {
    EntryLookupForm,
    VocabularyContext,
} from "../../../shared/components/entries/EntryLookupForm";

export const CreateEntryPage = () => {
    const { collectionId, vocabularyId } = entryCreateRouteApi.useParams();
    const navigate = useNavigate();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();
    const { raiseDuplicateEntryDialogAsync, dialogElement } =
        useDuplicateEntryDialog();

    const pendingRequestRef = useRef<CreateEntryData | null>(null);

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyDetailQuery(collectionId, vocabularyId);

    const createMutation = useCreateEntryMutation({
        onSuccess: () => {
            openSuccessNotification({ message: "Entry created successfully" });
            void navigate(vocabularyDetailPath(collectionId, vocabularyId));
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create entry. Please try again.",
            });
        },
        onDuplicateEntry: async (existingEntry) => {
            const addAnyway =
                await raiseDuplicateEntryDialogAsync(existingEntry);
            if (addAnyway && pendingRequestRef.current) {
                createMutation.mutate({
                    collectionId,
                    vocabularyId,
                    input: pendingRequestRef.current,
                    allowDuplicate: true,
                });
            }
        },
    });

    const handleSave = useCallback(
        (context: VocabularyContext | null, input: CreateEntryData) => {
            pendingRequestRef.current = input;
            createMutation.mutate({
                collectionId: context?.collectionId ?? collectionId,
                vocabularyId: context?.vocabularyId ?? vocabularyId,
                input,
            });
        },
        [collectionId, createMutation, vocabularyId]
    );

    const handleCancel = useCallback(() => {
        void navigate(vocabularyDetailPath(collectionId, vocabularyId));
    }, [collectionId, navigate, vocabularyId]);

    const handleLookupError = useCallback(
        (message: string) => {
            openErrorNotification({ message });
        },
        [openErrorNotification]
    );

    const renderContent = () => {
        if (isVocabularyLoading) return <ContentSkeleton variant="form" />;

        if (isVocabularyError || !vocabulary) {
            return (
                <RetryOnError
                    title="Failed to Load Data"
                    description="Something went wrong while loading the data. Please try again."
                    onRetry={() => void refetchVocabulary()}
                />
            );
        }

        return (
            <EntryLookupForm
                vocabularyId={vocabularyId}
                showVocabularySelector={false}
                isSaving={createMutation.isPending}
                onSave={handleSave}
                onCancel={handleCancel}
                onLookupError={handleLookupError}
                autoFocus={true}
                variant="page"
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
                    {
                        label: vocabulary?.name ?? "...",
                        ...vocabularyDetailPath(collectionId, vocabularyId),
                    },
                    { label: "New Entry" },
                ]}
            />
            <PageHeader title="Create Entry" />
            {renderContent()}
            {dialogElement}
        </PageContainer>
    );
};
