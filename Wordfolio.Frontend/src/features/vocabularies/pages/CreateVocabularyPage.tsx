import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { vocabularyCreateRouteApi, vocabularyDetailPath } from "../routes";
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
import { useCollectionQuery } from "../../collections/hooks/useCollectionQuery";
import { useCreateVocabularyMutation } from "../hooks/useCreateVocabularyMutation";
import { VocabularyForm } from "../components/VocabularyForm";
import { VocabularyFormData } from "../schemas/vocabularySchemas";

export const CreateVocabularyPage = () => {
    const { collectionId } = vocabularyCreateRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();
    const numericCollectionId = Number(collectionId);

    const {
        data: collection,
        isLoading,
        isError,
        refetch,
    } = useCollectionQuery(numericCollectionId);

    const createMutation = useCreateVocabularyMutation({
        onSuccess: (data) => {
            void navigate(vocabularyDetailPath(numericCollectionId, data.id));
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create vocabulary. Please try again.",
            });
        },
    });

    const handleSubmit = useCallback(
        (data: VocabularyFormData) => {
            createMutation.mutate({
                collectionId: numericCollectionId,
                request: {
                    name: data.name,
                    description: data.description,
                },
            });
        },
        [createMutation, numericCollectionId]
    );

    const handleCancel = useCallback(() => {
        void navigate(collectionDetailPath(numericCollectionId));
    }, [navigate, numericCollectionId]);

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="form" />;

        if (isError || !collection) {
            return (
                <RetryOnError
                    title="Failed to Load Collection"
                    description="Something went wrong while loading this collection. Please try again."
                    onRetry={() => void refetch()}
                />
            );
        }

        return (
            <VocabularyForm
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Create"
                isLoading={createMutation.isPending}
            />
        );
    }, [
        isLoading,
        isError,
        collection,
        refetch,
        handleSubmit,
        handleCancel,
        createMutation.isPending,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: collection?.name ?? "...",
                        ...collectionDetailPath(numericCollectionId),
                    },
                    { label: "New Vocabulary" },
                ]}
            />
            <PageHeader title="Create Vocabulary" />
            {renderContent()}
        </PageContainer>
    );
};
