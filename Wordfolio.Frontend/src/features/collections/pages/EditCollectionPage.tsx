import { useNavigate } from "@tanstack/react-router";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { CollectionForm } from "../components/CollectionForm";
import { useCollectionQuery } from "../hooks/useCollectionQuery";
import { useUpdateCollectionMutation } from "../hooks/useUpdateCollectionMutation";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { CollectionFormData } from "../schemas/collectionSchemas";
import {
    collectionEditRouteApi,
    collectionDetailPath,
    collectionsPath,
} from "../routes";

export const EditCollectionPage = () => {
    const { collectionId } = collectionEditRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification, openSuccessNotification } =
        useNotificationContext();
    const numericId = Number(collectionId);

    const {
        data: collection,
        isLoading,
        isError,
        refetch,
    } = useCollectionQuery(numericId);

    const mutation = useUpdateCollectionMutation(numericId, {
        onSuccess: () => {
            openSuccessNotification({
                message: "Collection updated successfully",
            });
            void navigate(collectionDetailPath(numericId));
        },
        onError: () => {
            openErrorNotification({ message: "Failed to update collection" });
        },
    });

    const handleSubmit = (data: CollectionFormData) => {
        mutation.mutate(data);
    };

    const handleCancel = () => {
        void navigate(collectionDetailPath(numericId));
    };

    if (isLoading) {
        return (
            <PageContainer>
                <ContentSkeleton variant="form" />
            </PageContainer>
        );
    }

    if (isError || !collection) {
        return (
            <PageContainer>
                <RetryOnError
                    title="Failed to Load Collection"
                    description="Something went wrong while loading this collection."
                    onRetry={() => void refetch()}
                />
            </PageContainer>
        );
    }

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: collection.name,
                        ...collectionDetailPath(numericId),
                    },
                    { label: "Edit" },
                ]}
            />
            <PageHeader title={`Edit ${collection.name}`} />

            <CollectionForm
                defaultValues={{
                    name: collection.name,
                    description: collection.description ?? undefined,
                }}
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Save Changes"
                isLoading={mutation.isPending}
            />
        </PageContainer>
    );
};
