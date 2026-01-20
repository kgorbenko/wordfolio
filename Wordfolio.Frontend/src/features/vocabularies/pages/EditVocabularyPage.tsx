import { useCallback } from "react";
import { useNavigate, useParams } from "@tanstack/react-router";

import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useCollectionQuery } from "../../collections/hooks/useCollectionQuery";
import { useVocabularyQuery } from "../hooks/useVocabularyQuery";
import { useUpdateVocabularyMutation } from "../hooks/useUpdateVocabularyMutation";
import { VocabularyForm } from "../components/VocabularyForm";
import { VocabularyFormData } from "../schemas/vocabularySchemas";

export const EditVocabularyPage = () => {
    const { collectionId, vocabularyId } = useParams({ strict: false });
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
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

    const updateMutation = useUpdateVocabularyMutation({
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
                message: "Failed to update vocabulary. Please try again.",
            });
        },
    });

    const handleSubmit = useCallback(
        (data: VocabularyFormData) => {
            updateMutation.mutate({
                collectionId: numericCollectionId,
                vocabularyId: numericVocabularyId,
                request: {
                    name: data.name,
                    description: data.description,
                },
            });
        },
        [updateMutation, numericCollectionId, numericVocabularyId]
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
            <VocabularyForm
                defaultValues={{
                    name: vocabulary.name,
                    description: vocabulary.description ?? undefined,
                }}
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Save"
                isLoading={updateMutation.isPending}
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
        handleSubmit,
        handleCancel,
        updateMutation.isPending,
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
                    { label: "Edit" },
                ]}
            />
            <PageHeader title="Edit Vocabulary" />
            {renderContent()}
        </PageContainer>
    );
};
