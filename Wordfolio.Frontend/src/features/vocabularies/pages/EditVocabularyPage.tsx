import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { vocabularyEditRouteApi, vocabularyDetailPath } from "../routes";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useVocabularyDetailQuery } from "../../../shared/queries/useVocabularyDetailQuery";
import { useUpdateVocabularyMutation } from "../hooks/useUpdateVocabularyMutation";
import { VocabularyForm } from "../components/VocabularyForm";
import { VocabularyFormData } from "../schemas/vocabularySchemas";

export const EditVocabularyPage = () => {
    const { collectionId, vocabularyId } = vocabularyEditRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyDetailQuery(collectionId, vocabularyId);

    const updateMutation = useUpdateVocabularyMutation({
        onSuccess: () => {
            void navigate(vocabularyDetailPath(collectionId, vocabularyId));
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to update vocabulary. Please try again.",
            });
        },
    });

    const handleSubmit = useCallback(
        (data: VocabularyFormData) => {
            updateMutation.mutate({ collectionId, vocabularyId, data });
        },
        [updateMutation, collectionId, vocabularyId]
    );

    const handleCancel = useCallback(() => {
        void navigate(vocabularyDetailPath(collectionId, vocabularyId));
    }, [navigate, collectionId, vocabularyId]);

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
        isVocabularyLoading,
        isVocabularyError,
        vocabulary,
        refetchVocabulary,
        handleSubmit,
        handleCancel,
        updateMutation.isPending,
    ]);

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
                    { label: "Edit" },
                ]}
            />
            <PageHeader title="Edit Vocabulary" />
            {renderContent()}
        </PageContainer>
    );
};
