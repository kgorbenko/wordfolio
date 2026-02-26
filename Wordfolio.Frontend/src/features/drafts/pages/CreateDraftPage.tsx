import { useCallback, useRef } from "react";
import { useNavigate } from "@tanstack/react-router";

import { draftsPath } from "../routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { useDuplicateEntryDialog } from "../../entries/hooks/useDuplicateEntryDialog";
import { useCreateDraftMutation } from "../hooks/useCreateDraftMutation";
import { CreateEntryRequest } from "../../entries/api/entriesApi";
import {
    EntryLookupForm,
    VocabularyContext,
} from "../../entries/components/EntryLookupForm";

export const CreateDraftPage = () => {
    const navigate = useNavigate();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();
    const { raiseDuplicateEntryDialogAsync, dialogElement } =
        useDuplicateEntryDialog();
    const pendingRequestRef = useRef<CreateEntryRequest | null>(null);

    const createMutation = useCreateDraftMutation({
        onSuccess: () => {
            openSuccessNotification({ message: "Draft created successfully" });
            void navigate(draftsPath());
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create draft. Please try again.",
            });
        },
        onDuplicateEntry: async (existingEntry) => {
            const addAnyway =
                await raiseDuplicateEntryDialogAsync(existingEntry);
            if (addAnyway && pendingRequestRef.current) {
                createMutation.mutate({
                    ...pendingRequestRef.current,
                    allowDuplicate: true,
                });
            }
        },
    });

    const handleSave = useCallback(
        (_: VocabularyContext | null, request: CreateEntryRequest) => {
            pendingRequestRef.current = request;
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
            {dialogElement}
        </PageContainer>
    );
};
