import { useNavigate } from "@tanstack/react-router";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";
import { CollectionForm } from "../components/CollectionForm";
import { useCreateCollectionMutation } from "../hooks/useCreateCollectionMutation";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { CollectionFormData } from "../schemas/collectionSchemas";
import { collectionsPath } from "../routes";

export const CreateCollectionPage = () => {
    const navigate = useNavigate();
    const { openErrorNotification, openSuccessNotification } =
        useNotificationContext();

    const mutation = useCreateCollectionMutation({
        onSuccess: async () => {
            openSuccessNotification({
                message: "Collection created successfully",
            });
            await navigate(collectionsPath());
        },
        onError: () => {
            openErrorNotification({ message: "Failed to create collection" });
        },
    });

    const handleSubmit = (data: CollectionFormData) => {
        mutation.mutate(data);
    };

    const handleCancel = () => {
        void navigate(collectionsPath());
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs
                items={[
                    { label: "Collections", ...collectionsPath() },
                    { label: "New Collection" },
                ]}
            />
            <PageHeader title="New Collection" />

            <CollectionForm
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Create Collection"
                isLoading={mutation.isPending}
            />
        </PageContainer>
    );
};
