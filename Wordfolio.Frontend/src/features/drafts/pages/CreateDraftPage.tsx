import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { draftsPath } from "../../../routes/_authenticated/drafts/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useCreateEntryMutation } from "../../entries/hooks/useCreateEntryMutation";
import { CreateEntryRequest } from "../../entries/api/entriesApi";
import { EntryLookupForm } from "../../entries/components/EntryLookupForm";

export const CreateDraftPage = () => {
    const navigate = useNavigate();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();

    const createMutation = useCreateEntryMutation({
        onSuccess: () => {
            openSuccessNotification({ message: "Draft created successfully" });
            void navigate(draftsPath());
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create draft. Please try again.",
            });
        },
    });

    const handleSave = useCallback(
        (request: CreateEntryRequest) => {
            createMutation.mutate(request);
        },
        [createMutation]
    );

    const handleCancel = useCallback(() => {
        void navigate(draftsPath());
    }, [navigate]);

    const handleLookupError = useCallback(
        (message: string) => {
            openErrorNotification({ message });
        },
        [openErrorNotification]
    );

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Drafts", ...draftsPath() },
                    { label: "New Draft" },
                ]}
            />
            <PageHeader title="Create Draft" />
            <EntryLookupForm
                showVocabularySelector={false}
                isSaving={createMutation.isPending}
                onSave={handleSave}
                onCancel={handleCancel}
                onLookupError={handleLookupError}
                autoFocus={true}
                variant="page"
            />
        </PageContainer>
    );
};
