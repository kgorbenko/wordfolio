import { useCallback } from "react";
import { useNavigate, useParams } from "@tanstack/react-router";

import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useCollectionQuery } from "../../collections/hooks/useCollectionQuery";
import { useVocabularyQuery } from "../../vocabularies/hooks/useVocabularyQuery";
import { useCreateEntryMutation } from "../../entries/hooks/useCreateEntryMutation";
import { CreateEntryRequest } from "../../entries/api/entriesApi";
import { EntryLookupForm } from "../../../components/common/EntryLookupForm";

export const CreateEntryPage = () => {
    const { collectionId, vocabularyId } = useParams({ strict: false });
    const navigate = useNavigate();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);

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

    const createMutation = useCreateEntryMutation({
        onSuccess: () => {
            openSuccessNotification({ message: "Entry created successfully" });
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
        void navigate({
            to: "/collections/$collectionId/$vocabularyId",
            params: {
                collectionId: String(collectionId),
                vocabularyId: String(vocabularyId),
            },
        });
    }, [navigate, collectionId, vocabularyId]);

    const handleLookupError = useCallback(
        (message: string) => {
            openErrorNotification({ message });
        },
        [openErrorNotification]
    );

    const isLoading = isCollectionLoading || isVocabularyLoading;
    const isError = isCollectionError || isVocabularyError;

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="form" />;

        if (isError || !collection || !vocabulary) {
            const handleRetry = () => {
                if (isCollectionError) void refetchCollection();
                if (isVocabularyError) void refetchVocabulary();
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
        isLoading,
        isError,
        collection,
        vocabulary,
        isCollectionError,
        isVocabularyError,
        refetchCollection,
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
                    { label: "New Entry" },
                ]}
            />
            <PageHeader title="Create Entry" />
            {renderContent()}
        </PageContainer>
    );
};
