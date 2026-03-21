import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { vocabularyCreateRouteApi, vocabularyDetailPath } from "../routes";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useCreateVocabularyMutation } from "../hooks/useCreateVocabularyMutation";
import { useVocabularyCollectionQuery } from "../hooks/useVocabularyCollectionQuery";
import { VocabularyForm } from "../components/VocabularyForm";
import { VocabularyFormData } from "../schemas/vocabularySchemas";

export const CreateVocabularyPage = () => {
    const { collectionId } = vocabularyCreateRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();

    const {
        data: collection,
        isLoading,
        isError,
        refetch,
    } = useVocabularyCollectionQuery(collectionId);

    const createMutation = useCreateVocabularyMutation({
        onSuccess: async (data) => {
            await navigate(vocabularyDetailPath(collectionId, data.id));
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create vocabulary. Please try again.",
            });
        },
    });

    const handleSubmit = useCallback(
        (data: VocabularyFormData) => {
            createMutation.mutate({ collectionId, data });
        },
        [createMutation, collectionId]
    );

    const handleCancel = useCallback(() => {
        void navigate(collectionDetailPath(collectionId));
    }, [navigate, collectionId]);

    const renderContent = () => {
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
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: collection?.name ?? "...",
                        ...collectionDetailPath(collectionId),
                    },
                    { label: "New Vocabulary" },
                ]}
            />
            <PageHeader title="Create Vocabulary" />
            {renderContent()}
        </PageContainer>
    );
};
