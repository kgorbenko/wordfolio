import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { vocabularyEditRouteApi, vocabularyDetailPath } from "../routes";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useVocabularyQuery } from "../hooks/useVocabularyQuery";
import { useUpdateVocabularyMutation } from "../hooks/useUpdateVocabularyMutation";
import { VocabularyForm } from "../components/VocabularyForm";
import { VocabularyFormData } from "../schemas/vocabularySchemas";

export const EditVocabularyPage = () => {
    const { collectionId, vocabularyId } = vocabularyEditRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyQuery(numericCollectionId, numericVocabularyId);

    const updateMutation = useUpdateVocabularyMutation({
        onSuccess: () => {
            void navigate(
                vocabularyDetailPath(numericCollectionId, numericVocabularyId)
            );
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
        void navigate(
            vocabularyDetailPath(numericCollectionId, numericVocabularyId)
        );
    }, [navigate, numericCollectionId, numericVocabularyId]);

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
                        ...collectionDetailPath(numericCollectionId),
                    },
                    {
                        label: vocabulary?.name ?? "...",
                        ...vocabularyDetailPath(
                            numericCollectionId,
                            numericVocabularyId
                        ),
                    },
                    { label: "Edit" },
                ]}
            />
            <PageHeader title="Edit Vocabulary" />
            {renderContent()}
        </PageContainer>
    );
};
