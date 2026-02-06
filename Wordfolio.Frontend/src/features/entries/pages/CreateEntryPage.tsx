import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { entryCreateRouteApi } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/routes";
import { collectionsPath } from "../../../routes/_authenticated/collections/routes";
import { collectionDetailPath } from "../../../routes/_authenticated/collections/routes";
import { vocabularyDetailPath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useVocabularyQuery } from "../../vocabularies/hooks/useVocabularyQuery";
import { useCreateEntryMutation } from "../hooks/useCreateEntryMutation";
import { CreateEntryRequest } from "../api/entriesApi";
import { EntryLookupForm } from "../components/EntryLookupForm";

export const CreateEntryPage = () => {
    const { collectionId, vocabularyId } = entryCreateRouteApi.useParams();
    const navigate = useNavigate();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyQuery(numericCollectionId, numericVocabularyId);

    const createMutation = useCreateEntryMutation({
        onSuccess: () => {
            openSuccessNotification({ message: "Entry created successfully" });
            void navigate(
                vocabularyDetailPath(numericCollectionId, numericVocabularyId)
            );
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create entry. Please try again.",
            });
        },
    });

    const handleSave = useCallback(
        (request: CreateEntryRequest) => {
            createMutation.mutate({
                ...request,
                vocabularyId: numericVocabularyId,
            });
        },
        [createMutation, numericVocabularyId]
    );

    const handleCancel = useCallback(() => {
        void navigate(
            vocabularyDetailPath(numericCollectionId, numericVocabularyId)
        );
    }, [navigate, numericCollectionId, numericVocabularyId]);

    const handleLookupError = useCallback(
        (message: string) => {
            openErrorNotification({ message });
        },
        [openErrorNotification]
    );

    const renderContent = useCallback(() => {
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
                vocabularyId={numericVocabularyId}
                showVocabularySelector={false}
                isSaving={createMutation.isPending}
                onSave={handleSave}
                onCancel={handleCancel}
                onLookupError={handleLookupError}
                autoFocus={true}
                variant="page"
            />
        );
    }, [
        isVocabularyLoading,
        isVocabularyError,
        vocabulary,
        refetchVocabulary,
        numericVocabularyId,
        createMutation.isPending,
        handleSave,
        handleCancel,
        handleLookupError,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: vocabulary?.collectionName ?? "...",
                        ...collectionDetailPath(numericCollectionId),
                    },
                    {
                        label: vocabulary?.name ?? "...",
                        ...vocabularyDetailPath(
                            numericCollectionId,
                            numericVocabularyId
                        ),
                    },
                    { label: "New Entry" },
                ]}
            />
            <PageHeader title="Create Entry" />
            {renderContent()}
        </PageContainer>
    );
};
