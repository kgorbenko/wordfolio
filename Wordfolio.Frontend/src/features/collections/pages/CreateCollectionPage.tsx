import { useNavigate } from "@tanstack/react-router";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { CollectionForm } from "../components/CollectionForm";
import { useCreateCollectionMutation } from "../hooks/useCreateCollectionMutation";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { CollectionFormData } from "../schemas/collectionSchemas";

export const CreateCollectionPage = () => {
    const navigate = useNavigate();
    const { openErrorNotification, openSuccessNotification } =
        useNotificationContext();

    const mutation = useCreateCollectionMutation({
        onSuccess: () => {
            openSuccessNotification({
                message: "Collection created successfully",
            });
            void navigate({ to: "/collections" });
        },
        onError: () => {
            openErrorNotification({ message: "Failed to create collection" });
        },
    });

    const handleSubmit = (data: CollectionFormData) => {
        mutation.mutate(data);
    };

    const handleCancel = () => {
        void navigate({ to: "/collections" });
    };

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", to: "/collections" },
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
