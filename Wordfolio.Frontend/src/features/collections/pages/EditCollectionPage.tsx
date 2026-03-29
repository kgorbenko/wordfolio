import { useNavigate } from "@tanstack/react-router";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { CollectionForm } from "../components/CollectionForm";
import { useCollectionQuery } from "../../../shared/api/queries/collections";
import { useUpdateCollectionMutation } from "../../../shared/api/mutations/collections";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
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
    const {
        data: collection,
        isLoading,
        isError,
        refetch,
    } = useCollectionQuery(collectionId);

    const mutation = useUpdateCollectionMutation(collectionId, {
        onSuccess: async () => {
            openSuccessNotification({
                message: "Collection updated successfully",
            });
            await navigate(collectionDetailPath(collectionId));
        },
        onError: () => {
            openErrorNotification({ message: "Failed to update collection" });
        },
    });

    const handleSubmit = (data: CollectionFormData) => {
        mutation.mutate(data);
    };

    const handleCancel = () => {
        void navigate(collectionDetailPath(collectionId));
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
            <TopBarBreadcrumbs
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: collection.name,
                        ...collectionDetailPath(collectionId),
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
